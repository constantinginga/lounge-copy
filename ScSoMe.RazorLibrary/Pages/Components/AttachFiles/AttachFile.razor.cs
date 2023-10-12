using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace ScSoMe.RazorLibrary.Pages.Components.AttachFiles
{
    public partial class AttachFile
    {
        private static readonly IReadOnlyCollection<string> _imgTypes =
            new ReadOnlyCollection<string>(new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".svg",
                ".gif"
                                                    });
        private static readonly IReadOnlyCollection<string> _videoTypes =
            new ReadOnlyCollection<string>(new[]
            {
                ".mp4",
                ".mov",
                ".wav",
                ".mkv",
                ".avi",
                ".wmv"
                                                    });
        private static readonly string _azureKey = "DefaultEndpointsProtocol=https;AccountName=startupcentralstorage;AccountKey=yXGzR961ybN/2hikNwKgjlCslQwV7E8QsA8hF4e59T+siRCTytM9jB//zfJuTKRP42v1OP1pbg99Obt+kK5dYA==;EndpointSuffix=core.windows.net";
        private const int MAX_ALLOWED_FILES = 5;
        public IList<IBrowserFile> medias = new List<IBrowserFile>();

        private void OnInputFileChange(InputFileChangeEventArgs e)
        {
            foreach (var item in e.GetMultipleFiles(MAX_ALLOWED_FILES))
            {

                if (medias.Count > 0 && medias.Any(x => x.Name.Equals(item.Name)))
                {
                    Snackbar.Add("File was already added", Severity.Warning);
                }
                else if (!_videoTypes.Contains(Path.GetExtension(item.Name)) && !_imgTypes.Contains(Path.GetExtension(item.Name)))
                {
                    Snackbar.Add("Invalid file format. Only images and videos are supported.", Severity.Warning);
                }
                else
                {
                    medias.Add(item);
                    AppState.IsMediaAttached = true;
                }

            }
        }

        public void ClearUploadedFiles()
        {
            medias.Clear();
            AppState.IsMediaAttached = false;
        }

        public async Task UploadToServer(IList<IBrowserFile> files, long postId)
        {
            try
            {


                var container = new BlobContainerClient(_azureKey, "upload-container");
                var createResponse = await container.CreateIfNotExistsAsync();
                if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                    await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

                foreach (var file in files)
                {
                    string blobName = $"/{postId}/{file.Name.Replace(" ", "-")}";
                    var blob = container.GetBlobClient(blobName);
                    await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                    long maxFileSize = (long)(4 * Math.Pow(10, 8));
                    using (var fileStream = file.OpenReadStream(maxFileSize))
                    {
                        await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = file.ContentType });
                    }
                    Console.WriteLine(blob.Uri.ToString());
                    Snackbar.Add($"{file.Name} was uploaded", Severity.Success);
                }
                medias.Clear();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Internal server error: {ex}");
                Snackbar.Add("Not Uploaded", Severity.Error);
            }


            #region old
            //var client = new HttpClient();

            //var response = await client.PostAsync($"https://localhost:7134/api/Files/FileUpload/{postId}", content);  //Change On Production

            //if (response.IsSuccessStatusCode)
            //{
            //    var options = new JsonSerializerOptions
            //    {
            //        PropertyNameCaseInsensitive = true,
            //    };

            //    using var responseStream =
            //        await response.Content.ReadAsStreamAsync();

            //var newUploadResults = await JsonSerializer
            //    .DeserializeAsync<IList<UploadInfo>>(responseStream, options);

            //if (newUploadResults is not null)
            //{
            //    uploadResults = uploadResults.Concat(newUploadResults).ToList();
            //}

            #endregion




        }

        private void RemoveItem(IBrowserFile file)
        {
            medias.Remove(file);
            if (medias.Count == 0) AppState.IsMediaAttached = false;
        }


        public void AddSnackbar(bool result)
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            if (result)
            {


                Snackbar.Add("Files Uploaded Successfuly", Severity.Success);

            }
            else
            {
                Snackbar.Add("Something went wrong with uploading your files", Severity.Error);
            }
        }

        public IList<IBrowserFile> GetMedias()
        {
            return medias;
        }
    }
}
