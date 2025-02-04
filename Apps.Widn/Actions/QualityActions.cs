using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apps.Widn.Invocables;
using Apps.Widn.Models.Requests;
using Apps.Widn.Models.Responses;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Actions;
using Blackbird.Applications.Sdk.Common.Invocation;
using Blackbird.Applications.SDK.Extensions.FileManagement.Interfaces;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using RestSharp;

namespace Apps.Widn.Actions
{
    [ActionList]
    public class QualityActions : WidnInvocable
    {
        private readonly IFileManagementClient _fileManagementClient;
        public QualityActions(InvocationContext invocationContext, IFileManagementClient fileManagementClient) : base(invocationContext)
        {
            _fileManagementClient = fileManagementClient;
        }

        [Action("Estimate quality", Description = "Evaluates the quality of a translation")]
        public async Task<QualityEvaluateResponse> GetQuality([ActionParameter] QualityEvaluateRequest input)
        {
            var requestBody = new
            {
                segments = new[]
                {
                    new
                    {
                        sourceText = input.SourceText,
                        targetText = input.TargetText,
                        referenceText = input.ReferenceText
                    }
                },
                model = "xcomet-xl"
            };

            var restRequest = new RestRequest("/quality/evaluate", Method.Post);
            restRequest.AddJsonBody(requestBody);
            var response = await Client.ExecuteWithErrorHandling<QualityEvaluateResponse>(restRequest);

            return response;
        }

    }
}
