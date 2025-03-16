using Moq;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Features.Flights.Models;
using Wingman.Api.Features.Flights.Repositories.Interfaces;
using Wingman.Api.Features.Flights.Services;
using Wingman.Tests.Unit.Core;

namespace Wingman.Tests.Unit.Features;

public class FlightsServiceTests : CommonServiceTests<Flight>
{
    private static readonly Mock<IFlightsRepository> _mockRepo = new Mock<IFlightsRepository>();

    public FlightsServiceTests() : base(new FlightsService(_mockRepo.Object), _mockRepo.As<ICommonRepository<Flight>>())
    {
    }
}
