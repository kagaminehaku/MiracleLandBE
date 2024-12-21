﻿using Microsoft.Extensions.Options;

namespace MiracleLandBE.LogicalServices
{
    public class ImgbbSettings
    {
        public string ApiKey { get; set; }
    }

    public class ImageUploader
    {
        public class ImgbbUploader
        {
            private readonly string apiKey;

            public ImgbbUploader(IOptions<ImgbbSettings> imgbbSettings)
            {
                this.apiKey = imgbbSettings.Value.ApiKey;
            }

            public async Task<string> UploadImageAsync(string imagePath)
            {
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        byte[] imageData = File.ReadAllBytes(imagePath);
                        var content = new MultipartFormDataContent();
                        content.Add(new StringContent(apiKey), "key");
                        content.Add(new ByteArrayContent(imageData), "image", Path.GetFileName(imagePath)); // Image file
                        var response = await httpClient.PostAsync("https://api.imgbb.com/1/upload", content);
                        response.EnsureSuccessStatusCode();
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        dynamic responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                        if (responseObject.success == true)
                        {
                            string imageUrl = responseObject.data.url;
                            return imageUrl;
                        }
                        else
                        {
                            string errorMessage = responseObject.error.message;
                            throw new Exception($"Image upload failed: {errorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"An error occurred while uploading the image: {ex.Message}");
                    }
                }
            }
        }
    }
}
