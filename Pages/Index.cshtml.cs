
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Storage.Queues;

public class IndexModel : PageModel
{
    private readonly IConfiguration _config;

    public IndexModel(IConfiguration config)
    {
        _config = config;
    }

    [BindProperty]
    public string Message { get; set; } = "";

    public string Result { get; set; } = "";

    private QueueClient GetQueue()
    {
        string connection = _config["AzureStorage:ConnectionString"]!;
        string name = _config["AzureStorage:QueueName"]!;

        QueueClient queue = new QueueClient(connection, name);

        queue.CreateIfNotExists();

        return queue;
    }

    public async Task<IActionResult> OnPostInsertAsync()
    {
        var queue = GetQueue();

        await queue.SendMessageAsync(Message);

        Result = "Message inserted successfully!";

        return Page();
    }

    public async Task<IActionResult> OnPostGetAsync()
    {
        var queue = GetQueue();

        var response = await queue.ReceiveMessageAsync();

        if (response.Value != null)
        {
            Result = response.Value.MessageText;

            await queue.DeleteMessageAsync(
                response.Value.MessageId,
                response.Value.PopReceipt
            );
        }
        else
        {
            Result = "Queue is empty!";
        }

        return Page();
    }
}
