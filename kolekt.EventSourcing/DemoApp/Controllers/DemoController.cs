using System;
using System.ComponentModel.DataAnnotations;
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

namespace DemoApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        private readonly IMessageBus _messageBus;
        private readonly ICommandValidator _commandValidator;
        private readonly IQueryHandler<GetDemoByIdQuery, DemoAggregate> _findDemoQueryHandler;

        public DemoController(IMessageBus messageBus,
            ICommandValidator commandValidator,
            IQueryHandler<GetDemoByIdQuery, DemoAggregate> findDemoQueryHandler)
        {
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
                var response = await _messageBus.SendCommandAsync<CreateAggregateCommand, DemoAggregate>(command);

                if (response.Success)
                {
                    return Ok(response.Data);
                }
                else
                {
                    ModelState.AddModelError("", response.ErrorMessage);
                    return UnprocessableEntity(ModelState);
                }
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
                var response = await _messageBus.SendCommandAsync<DoSomethingCommand, DemoAggregate>(command);
                if (response.Success)
                {
                    return Ok(response.Data);
                }
                else
                {
                    ModelState.AddModelError("", response.ErrorMessage);
                    return UnprocessableEntity(ModelState);
                }
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDemo([FromQuery][Required]Guid id)
        {
            var demo = await _findDemoQueryHandler.QueryAsync(new GetDemoByIdQuery
            {
                DemoId = id
            });

            if (demo != null)
            {
                return Ok(demo);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete]
        [Route("")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeleteDemo([FromForm][Required]Guid id)
        {
            var command = new DeleteAggregateCommand()
            {
                DemoId = id
            };
            var validation = await _commandValidator.Validate(command);
            if (validation.IsValid)
            {
                await _messageBus.SendCommandAsync(command);
                return NoContent();
            }
            else
            {
                ModelState.AddModelError(validation.Property, validation.ErrorMessage);
                return UnprocessableEntity(ModelState);
            }
        }
    }
}
