namespace Core.Hangfire;

public class WorkerConstants
{
    public const int TotalRetry = 2;
    
    public static class Queues
    {
        public const string Dev = "dev";
        public const string Scraping = "scraping";
        public const string ServerScraping = "server-scraping";
    }
}