using Firebase.Storage;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class FirebaseStorageService
    {
        // private const string BucketName = "ev-marketplace.appspot.com";
        private const string BucketName = ("ev-marketplace-9b1f7.firebasestorage.app");

        private readonly string _credsPath;

        public FirebaseStorageService(IWebHostEnvironment env)
        {
            // file JSON để ở cùng cấp Program.cs (API project)
            _credsPath = Path.Combine(env.ContentRootPath, "firebase-adminsdk.json");
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
        {
            try
            {
                // Lấy access token từ service account
                var credential = GoogleCredential.FromFile(_credsPath)
                    .CreateScoped("https://www.googleapis.com/auth/devstorage.read_write");

                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                var task = new FirebaseStorage(
                                BucketName,
                                new FirebaseStorageOptions
                                {
                                    ThrowOnCancel = true,
                                    // 👇 cấp token cho FirebaseStorage.net
                                    AuthTokenAsyncFactory = () => Task.FromResult(accessToken)
                                })
                            .Child("images")
                            .Child(fileName)
                            .PutAsync(fileStream, ct, contentType);

                return await task; // trả về download URL
            } catch(Exception ex) {                 // Xử lý lỗi upload
                throw new Exception("Failed to upload file to Firebase Storage", ex);
            }

        }
        public async Task DeleteFileAsync(string imageUrl, CancellationToken ct = default)
        {
            try
            {
                // Lấy access token
                var credential = GoogleCredential.FromFile(_credsPath)
                    .CreateScoped("https://www.googleapis.com/auth/devstorage.read_write");

                var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

                // 🔹 FirebaseStorage cần biết đường dẫn tương đối (child path)
                // Ví dụ: https://firebasestorage.googleapis.com/v0/b/<bucket>/o/images%2Fabc.png?alt=media
                // → cần cắt lấy "images/abc.png"
                var fileName = GetRelativePathFromUrl(imageUrl);

                var task = new FirebaseStorage(
                                BucketName,
                                new FirebaseStorageOptions
                                {
                                    ThrowOnCancel = true,
                                    AuthTokenAsyncFactory = () => Task.FromResult(accessToken)
                                })
                            .Child(fileName)
                            .DeleteAsync();

                await task;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete file from Firebase: {ex.Message}", ex);
            }
        }

        // 🧩 Hàm phụ: tách path từ URL (ví dụ "images/abc.png")
        private string GetRelativePathFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";

            // Firebase encode path: "images%2Fabc.png" → "images/abc.png"
            var start = url.IndexOf("/o/") + 3;
            var end = url.IndexOf("?alt=");

            if (start < 0 || end < 0 || end <= start) return "";

            var encodedPath = url.Substring(start, end - start);
            return Uri.UnescapeDataString(encodedPath);
        }

    }
}
