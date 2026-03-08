using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaEmail;

// Só Type — Email removido, SNS gerencia os destinatários
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
            context.Logger.LogInformation($"Mensagem recebida do SQS: {record.Body}");

            var input = JsonSerializer.Deserialize<EmailRequest>(
                record.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (input == null || string.IsNullOrEmpty(input.Type))
            {
                context.Logger.LogWarning("Type não informado na mensagem.");
                continue;
            }

            string subject;
            string message;

            if (input.Type == "welcome")
            {
                subject = "Boas-vindas à plataforma";
                message = "Bem-vindo à plataforma! Seu cadastro foi realizado com sucesso.";
            }
            else if (input.Type == "payment")
            {
                subject = "Pagamento aprovado";
                message = "Seu pagamento foi aprovado com sucesso. Obrigado pela compra!";
            }
            else
            {
                context.Logger.LogWarning($"Tipo inválido: {input.Type}");
                continue;
            }

            await _snsClient.PublishAsync(new PublishRequest
            {
                TopicArn = _topicArn,
                Subject = subject,
                Message = message
            });

            context.Logger.LogInformation($"Notificação SNS publicada. Tipo: '{input.Type}'");
        }
    }
}