using Amazon.Lambda.Core;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaEmail;

public class EmailRequest
{
    public string Type { get; set; } = "";
    public string Email { get; set; } = "";
}

public class Function
{
    private readonly AmazonSimpleEmailServiceV2Client _sesClient;

    public Function()
    {
        _sesClient = new AmazonSimpleEmailServiceV2Client();
    }

    public async Task<string> FunctionHandler(EmailRequest input, ILambdaContext context)
    {
        if (string.IsNullOrEmpty(input.Email))
            return "Email não informado";

        string subject;
        string body;

        if (input.Type == "welcome")
        {
            subject = "Boas-vindas";
            body = "Bem-vindo à plataforma!";
        }
        else if (input.Type == "payment")
        {
            subject = "Pagamento aprovado";
            body = "Seu pagamento foi aprovado com sucesso.";
        }
        else
        {
            return "Tipo de email inválido";
        }

        var request = new SendEmailRequest
        {
            FromEmailAddress = "daiana.russi.desenvolvedora@gmail.com",
            Destination = new Destination
            {
                ToAddresses = new List<string>
                {
                    input.Email
                }
            },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content
                    {
                        Data = subject
                    },
                    Body = new Body
                    {
                        Text = new Content
                        {
                            Data = body
                        }
                    }
                }
            }
        };

        await _sesClient.SendEmailAsync(request);

        return "Email enviado com sucesso";
    }
}