using AnonymizationTool.Data.Persistence;
using AnonymizationTool.Export.SchulIT.Idp.Request;
using AnonymizationTool.Messages;
using AnonymizationTool.Settings;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AnonymizationTool.Export.SchulIT.Idp
{
    public class IdpExportService : IExportService
    {
        private const int MaxConcurrentRequests = 10;

        public event ProgressChangedEventHandler ProgressChanged;

        protected virtual void OnRaiseProgressChangedEvent(ProgressChangedEventArgs args)
        {
            ProgressChanged?.Invoke(this, args);
        }

        private readonly ISettingsService settingsService;
        private readonly IMessenger messenger;

        public IdpExportService(ISettingsService settingsService, IMessenger messenger)
        {
            this.settingsService = settingsService;
            this.messenger = messenger;
        }

        public async Task ExportAsync(IEnumerable<AnonymousStudent> students)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(settingsService.Settings.Export.SchulITIdp.Endpoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Token", settingsService.Settings.Export.SchulITIdp.Token);

                var current = 0;
                var total = students.Count();

                while (current < total)
                {
                    var requests = new List<Task<HttpResponseMessage>>();

                    for (int currentIdx = current; currentIdx < Math.Min(current + MaxConcurrentRequests, total); currentIdx++)
                    {
                        var student = students.ElementAt(currentIdx);

                        var request = new UserAttributes();

                        if (!string.IsNullOrEmpty(settingsService.Settings.Export.SchulITIdp.FirstnameAttributeName))
                        {
                            request.Attributes.Add(settingsService.Settings.Export.SchulITIdp.FirstnameAttributeName, student.AnonymousFirstName);
                        }

                        if (!string.IsNullOrEmpty(settingsService.Settings.Export.SchulITIdp.LastnameAttributeName))
                        {
                            request.Attributes.Add(settingsService.Settings.Export.SchulITIdp.LastnameAttributeName, student.AnonymousLastName);
                        }

                        if (!string.IsNullOrEmpty(settingsService.Settings.Export.SchulITIdp.EmailAttributeName))
                        {
                            request.Attributes.Add(settingsService.Settings.Export.SchulITIdp.EmailAttributeName, student.AnonymousEmail);
                        }

                        if (request.Attributes.Count == 0)
                        {
                            continue;
                        }

                        var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        requests.Add(client.PatchAsync($"/api/user/{student.SchILDId}/attributes", content));
                    }

                    await Task.WhenAll(requests);

                    current += requests.Count;

                    OnRaiseProgressChangedEvent(new ProgressChangedEventArgs(current, total, $"Aktualisiere Benutzer ({current}/{total})..."));

                    var responses = requests.Where(x => x.IsCompleted).Select(x => x.Result);
                    var success = responses.Where(x => x.IsSuccessStatusCode);
                    var failures = responses.Where(x => !x.IsSuccessStatusCode);

                    if (failures.Any())
                    {
                        var firstFailure = failures.First();
                        var message = $"Gemeldeter Status-Code: {firstFailure.StatusCode}";
                        var content = await firstFailure.Content.ReadAsStringAsync();

                        if (!string.IsNullOrEmpty(content))
                        {
                            message = content;
                        }

                        throw new Exception(message);
                    }
                }
            }
        }
    }
}
