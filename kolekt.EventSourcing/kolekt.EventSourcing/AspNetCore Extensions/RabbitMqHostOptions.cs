using System;
using System.Collections.Generic;
using System.Text;

namespace kolekt.EventSourcing.Extensions
{
    public class RabbitMqHostOptions
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
