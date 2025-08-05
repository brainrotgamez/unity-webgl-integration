using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace VeryRareVentures
{

    [System.Serializable]
    public class InventoryItem
    {
        public string id;
        public string name;
        public string description;
        public string imageUrl;
        public string templateId;
        public string origin;
        public bool isListed;
        public string multimediaUrl;
        public List<ItemAttribute> attributes;
        // Add additional fields as needed
    }
        [System.Serializable]
    public class ItemAttribute
    {
        public string name;
        public string value;
    }

    [System.Serializable]
    public class InventoryResponseData
    {
        public int total;
        public List<InventoryItem> items;
        public bool hasMore;
    }
    /// <summary>
    /// Provides custom serialization helpers for inventory data
    /// </summary>
    public static class VRVInventorySerializer
    {
        /// <summary>
        /// Parse the raw JSON response from the server API
        /// </summary>
        public static InventoryResponseData ParseInventoryResponse(string jsonResponse)
        {
            Debug.Log($"[VRVInventorySerializer] Parsing raw inventory response: {jsonResponse}");
            Debug.Log($"[VRVInventorySerializer] JSON length: {jsonResponse?.Length ?? 0} characters");
            
            try
            {
                // Use JsonUtility as first approach
                InventoryResponseData response = JsonUtility.FromJson<InventoryResponseData>(jsonResponse);
                
                // If parsing succeeded and we have items, return the result
                if (response != null && response.items != null && response.items.Count > 0)
                {
                    Debug.Log($"[VRVInventorySerializer] Successfully parsed {response.items.Count} items with JsonUtility");
                    return response;
                }
                else
                {
                    Debug.Log($"[VRVInventorySerializer] JsonUtility result: response={response != null}, items={response?.items != null}, count={response?.items?.Count ?? 0}");
                }
                
                // If JsonUtility failed (common with some JSON structures), use manual parsing
                Debug.Log($"[VRVInventorySerializer] JsonUtility parsing failed or returned no items, trying manual parsing");
                return ManuallyParseInventoryJSON(jsonResponse);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VRVInventorySerializer] Error parsing inventory JSON: {ex.Message}");
                Debug.LogError($"[VRVInventorySerializer] Stack trace: {ex.StackTrace}");
                
                // Try manual parsing as fallback
                return ManuallyParseInventoryJSON(jsonResponse);
            }
        }
        
        /// <summary>
        /// Manual parsing for when JsonUtility fails with complex JSON structures
        /// </summary>
        private static InventoryResponseData ManuallyParseInventoryJSON(string jsonResponse)
        {
            Debug.Log("[VRVInventorySerializer] Starting manual parsing...");
            
            try
            {
                // Create a new response object
                InventoryResponseData response = new InventoryResponseData();
                response.items = new List<InventoryItem>();
                
                // Extract the total count
                Match totalMatch = Regex.Match(jsonResponse, "\"total\"\\s*:\\s*(\\d+)");
                if (totalMatch.Success)
                {
                    response.total = int.Parse(totalMatch.Groups[1].Value);
                    Debug.Log($"[VRVInventorySerializer] Found total: {response.total}");
                }
                else
                {
                    response.total = 0;
                    Debug.Log("[VRVInventorySerializer] No total found, defaulting to 0");
                }
                
                // Find the items array
                Match itemsMatch = Regex.Match(jsonResponse, "\"items\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
                if (!itemsMatch.Success)
                {
                    Debug.LogWarning("[VRVInventorySerializer] No 'items' array found in JSON");
                    return response;
                }
                
                string itemsContent = itemsMatch.Groups[1].Value;
                Debug.Log($"[VRVInventorySerializer] Found items array content: {itemsContent.Substring(0, Math.Min(200, itemsContent.Length))}...");
                
                // Split items by top-level commas (but not commas inside nested objects)
                List<string> itemStrings = SplitJsonArray(itemsContent);
                Debug.Log($"[VRVInventorySerializer] Split into {itemStrings.Count} item(s)");
                
                foreach (string itemJson in itemStrings)
                {
                    if (string.IsNullOrWhiteSpace(itemJson)) continue;
                    
                    Debug.Log($"[VRVInventorySerializer] Processing item: {itemJson.Substring(0, Math.Min(100, itemJson.Length))}...");
                    
                    string actualItemJson = itemJson.Trim();
                    
                    // Check if this is a nested structure (has numbered keys like "0", "1", etc.)
                    Match nestedMatch = Regex.Match(actualItemJson, "\"\\d+\"\\s*:\\s*\\{(.*)\\}\\s*$", RegexOptions.Singleline);
                    if (nestedMatch.Success)
                    {
                        // Extract the actual item data from inside the numbered key
                        actualItemJson = "{" + nestedMatch.Groups[1].Value + "}";
                        Debug.Log($"[VRVInventorySerializer] Found nested item structure, extracted: {actualItemJson.Substring(0, Math.Min(150, actualItemJson.Length))}...");
                    }
                    
                    // Create inventory item
                    InventoryItem item = new InventoryItem();
                    
                    // Extract each field using improved regex patterns
                    item.id = ExtractJsonValue(actualItemJson, "id");
                    item.name = ExtractJsonValue(actualItemJson, "name");
                    item.description = ExtractJsonValue(actualItemJson, "description");
                    item.imageUrl = ExtractJsonValue(actualItemJson, "imageUrl");
                    item.multimediaUrl = ExtractJsonValue(actualItemJson, "multimediaUrl");
                    item.origin = ExtractJsonValue(actualItemJson, "origin");
                    
                    Debug.Log($"[VRVInventorySerializer] Extracted item data:");
                    Debug.Log($"[VRVInventorySerializer]   - ID: '{item.id}'");
                    Debug.Log($"[VRVInventorySerializer]   - Name: '{item.name}'");
                    Debug.Log($"[VRVInventorySerializer]   - ImageUrl: '{item.imageUrl}'");
                    Debug.Log($"[VRVInventorySerializer]   - MultimediaUrl: '{item.multimediaUrl}'");
                    
                    // Only add items that have both ID and name
                    if (!string.IsNullOrEmpty(item.id) && !string.IsNullOrEmpty(item.name))
                    {
                        response.items.Add(item);
                        Debug.Log($"[VRVInventorySerializer] ✓ Added item: {item.name} (ID: {item.id})");
                    }
                    else
                    {
                        Debug.LogWarning($"[VRVInventorySerializer] ✗ Skipped item due to missing ID or name");
                    }
                }
                
                Debug.Log($"[VRVInventorySerializer] Manual parsing complete. Found {response.items.Count} valid items");
                return response;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VRVInventorySerializer] Error in manual parsing: {e.Message}");
                Debug.LogError($"[VRVInventorySerializer] Stack trace: {e.StackTrace}");
                return new InventoryResponseData { items = new List<InventoryItem>() };
            }
        }
        
        /// <summary>
        /// Split a JSON array string into individual item strings, respecting nested braces
        /// </summary>
        private static List<string> SplitJsonArray(string arrayContent)
        {
            List<string> items = new List<string>();
            int braceLevel = 0;
            int startIndex = 0;
            
            for (int i = 0; i < arrayContent.Length; i++)
            {
                char c = arrayContent[i];
                
                if (c == '{')
                {
                    braceLevel++;
                }
                else if (c == '}')
                {
                    braceLevel--;
                }
                else if (c == ',' && braceLevel == 0)
                {
                    // Found a top-level comma, extract the item
                    string item = arrayContent.Substring(startIndex, i - startIndex).Trim();
                    if (!string.IsNullOrEmpty(item))
                    {
                        items.Add(item);
                    }
                    startIndex = i + 1;
                }
            }
            
            // Add the last item
            if (startIndex < arrayContent.Length)
            {
                string lastItem = arrayContent.Substring(startIndex).Trim();
                if (!string.IsNullOrEmpty(lastItem))
                {
                    items.Add(lastItem);
                }
            }
            
            return items;
        }
        
        /// <summary>
        /// Extract a string value from JSON for a given key
        /// </summary>
        private static string ExtractJsonValue(string json, string key)
        {
            // Try multiple patterns to handle different JSON formatting
            string[] patterns = {
                $"\"{key}\"\\s*:\\s*\"([^\"]*?)\"",  // Standard string value: "key": "value"
                $"\"{key}\"\\s*:\\s*([^,\\}}\\s]+)", // Non-quoted value: "key": value
                $"\"{key}\"\\s*:\\s*\"(.*?)\"(?=[,\\}}])" // String with lookahead: "key": "value",
            };
            
            foreach (string pattern in patterns)
            {
                Match match = Regex.Match(json, pattern);
                if (match.Success)
                {
                    string value = match.Groups[1].Value.Trim();
                    Debug.Log($"[VRVInventorySerializer] ExtractJsonValue('{key}'): Found '{value}' using pattern {Array.IndexOf(patterns, pattern) + 1}");
                    return value;
                }
            }
            
            Debug.Log($"[VRVInventorySerializer] ExtractJsonValue('{key}'): No match found");
            
            // Debug: Show the area around where we expected to find the key
            int keyIndex = json.IndexOf($"\"{key}\"");
            if (keyIndex >= 0)
            {
                int start = Math.Max(0, keyIndex - 50);
                int end = Math.Min(json.Length, keyIndex + 150);
                string context = json.Substring(start, end - start);
                Debug.Log($"[VRVInventorySerializer] Context around '{key}': ...{context}...");
            }
            else
            {
                Debug.Log($"[VRVInventorySerializer] Key '{key}' not found in JSON at all");
            }
            
            return "";
        }
    }
}