# FCG Notifications Lambda

Este projeto contém uma função AWS Lambda em C# (.NET) responsável por processar requisições de notificação a partir de uma fila Amazon SQS e publicá-las em um tópico do Amazon SNS.

## Funcionalidades
- Consome eventos em lote de uma fila SQS.
- Extrai e processa contextos de rastreamento distribuído (Distributed Tracing) através do atributo de mensagem `traceparent` (Padrão W3C).
- Avalia o tipo de notificação recebida (atualmente `welcome` e `payment`).
- Publica a mensagem final formatada em um tópico do SNS.

## Variáveis de Ambiente

Para que a Lambda funcione corretamente e possa publicar mensagens, a seguinte variável de ambiente deve ser configurada:

| Variável | Descrição | Obrigatório |
|----------|-----------|-------------|
| `SNS_TOPIC_ARN` | O ARN (Amazon Resource Name) do tópico SNS de destino para onde os e-mails/notificações serão enviados. | Sim |

## Formato da Mensagem SQS

A Lambda espera que o corpo (*body*) da mensagem SQS contenha um JSON com a propriedade `Type`. A desserialização ignora diferenças de maiúsculas/minúsculas (*case-insensitive*).

### Exemplos de Payload

**Boas-vindas (`welcome`):**
```json
{
  "Type": "welcome"
}
```

**Pagamento Aprovado (`payment`):**
```json
{
  "Type": "payment"
}
```

> **Nota:** Qualquer valor de `Type` diferente de `welcome` ou `payment` (ou um payload vazio) gerará um log de alerta (*Warning*) e a mensagem não será enviada ao SNS.

## Dependências Principais
- `Amazon.Lambda.Core` e `Amazon.Lambda.SQSEvents`
- `Amazon.Lambda.Serialization.SystemTextJson`
- `AWSSDK.SimpleNotificationService`

## Como testar localmente
Você pode utilizar a AWS Lambda Test Tool ou a extensão do *AWS Toolkit* na sua IDE para simular o evento SQS. Lembre-se de configurar a variável de ambiente `SNS_TOPIC_ARN` no `launchSettings.json` ou no painel da ferramenta de teste antes de executar.