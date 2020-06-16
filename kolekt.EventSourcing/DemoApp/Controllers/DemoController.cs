using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using DemoApp.Aggregates;
using DemoApp.Messages;
using DemoApp.Queries;
using kolekt.EventSourcing.Consumers;
using kolekt.EventSourcing.Providers;
using kolekt.EventSourcing.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DemoApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        private readonly ILogger<DemoController> _logger;
        private readonly IMessageBus _messageBus;
        private readonly ICommandValidator _commandValidator;
        private readonly IQueryHandler<GetDemoByIdQuery, DemoAggregate> _findDemoQueryHandler;

        public DemoController(ILogger<DemoController> logger,
            IMessageBus messageBus,
            ICommandValidator commandValidator,
            IQueryHandler<GetDemoByIdQuery, DemoAggregate> findDemoQueryHandler)
        {
            _logger = logger;
            _messageBus = messageBus;
            _commandValidator = commandValidator;
            _findDemoQueryHandler = findDemoQueryHandler;
        }

        [HttpPost]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DemoAggregate), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionary), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateDemo([Required][FromForm] Guid id)
        {
            var command = new CreateAggregateCommand
            {
                DemoId = id
            };

            var validation = await _commandValidator.Validate(command);
            if (validation.IsValid)
            {
                var aggregate = await _messageBus.SendCommandAsync<CreateAggregateCommand, DemoAggregate>(command);

                return Ok(aggregate);
            }
            else
            {
                ModelState.AddModelError(validation.Property, validation.ErrorMessage);
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpPut("")]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DemoAggregate), StatusCodes.Status200OK)]
        public async Task<IActionResult> DoSomething([Required][FromForm]Guid id, [Required][FromForm]string messageText)
        {
            var command = new DoSomethingCommand
            {
                DemoId = id,
                MessageText = messageText
            };
            var validation = await _commandValidator.Validate(command);
            if (validation.IsValid)
            {
                var aggregate = await _messageBus.SendCommandAsync<DoSomethingCommand, DemoAggregate>(command);
                return Ok(aggregate);
            }
            else
            {
                ModelState.AddModelError(validation.Property, validation.ErrorMessage);
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpGet]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DemoAggregate), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDemo([FromQuery][Required]Guid id)
        {
            var demo = await _findDemoQueryHandler.QueryAsync(new GetDemoByIdQuery
            {
                DemoId = id
            });

            return Ok(demo);
        }
    }
}
