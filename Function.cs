using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Diagnostics;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaEmail;

public class EmailRequest
{
    public string Type { get; set; } = "";
}

public class Function
{
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly string _topicArn;

    public Function()
    {
        _snsClient = new AmazonSimpleNotificationServiceClient();
        _topicArn = Environment.GetEnvironmentVariable("SNS_TOPIC_ARN")
            ?? throw new InvalidOperationException("Variável SNS_TOPIC_ARN não configurada.");
    }

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var record in sqsEvent.Records)
        {
            var attrs = record.MessageAttributes ?? new Dictionary<string, SQSEvent.MessageAttribute>();
            attrs.TryGetValue("traceparent", out var traceParentAttr);
            var traceParent = traceParentAttr?.StringValue;
            string? traceId = null;

            if (!string.IsNullOrWhiteSpace(traceParent) &&
                ActivityContext.TryParse(traceParent, null, out var ctx))
            {
                traceId = ctx.TraceId.ToString();
            }

            context.Logger.LogInformation(
            $"Mensagem recebida do SQS. TraceId: {traceId}, Body: {record.Body}");


            var input = JsonSerializer.Deserialize<EmailRequest>(
                record.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (input == null || string.IsNullOrEmpty(input.Type))
            {
                context.Logger.LogWarning($"Type não informado na mensagem. TraceId: {traceId}");
                continue;
            }

            string subject;
            string message;

            if (input.Type == "welcome")
            {
                subject = "Boas-vindas a plataforma";
                message = "Bem-vindo a plataforma! Seu cadastro foi realizado com sucesso.";
            }
            else if (input.Type == "payment")
            {
                subject = "Pagamento aprovado";
                message = "Seu pagamento foi aprovado com sucesso. Obrigado pela compra!";
            }
            else
            {
                context.Logger.LogWarning($"Tipo inválido: {input.Type}. TraceId: {traceId}");
                continue;
            }

            await _snsClient.PublishAsync(new PublishRequest
            {
                TopicArn = _topicArn,
                Subject = subject,
                Message = message
            });

            context.Logger.LogInformation($"Notificação SNS publicada. Tipo: '{input.Type}', TraceId: {traceId}");
        }
    }
}
