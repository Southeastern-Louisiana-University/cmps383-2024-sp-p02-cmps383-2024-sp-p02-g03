using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Selu383.SP24.Tests.Controllers.Authentication;
using Selu383.SP24.Tests.Dtos;
using Selu383.SP24.Tests.Helpers;

namespace Selu383.SP24.Tests.Controllers.Hotels;

[TestClass]
public class HotelsControllerTests
{
    private WebTestContext context = new();

    [TestInitialize]
    public void Init()
    {
        context = new WebTestContext();
    }

    [TestCleanup]
    public void Cleanup()
    {
        context.Dispose();
    }

    [TestMethod]
    public async Task ListAllHotels_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/hotels");

        //assert
        await httpResponse.AssertHotelListAllFunctions();
    }

    [TestMethod]
    public async Task GetHotelById_Returns200AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetHotel();
        if (target == null)
        {
            Assert.Fail("Make List All hotels work first");
            return;
        }

        //act
        var httpResponse = await webClient.GetAsync($"/api/hotels/{target.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/hotels/{id} ");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<HotelDto>();
        resultDto.Should().BeEquivalentTo(target, "we expect get hotel by id to return the same data as the list all hotels endpoint");
    }

    [TestMethod]
    public async Task GetHotelById_NoSuchId_Returns404()
    {
        //arrange
        var webClient = context.GetStandardWebClient();

        //act
        var httpResponse = await webClient.GetAsync("/api/hotels/999999");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling GET /api/hotels/{id} with an invalid id");
    }

    [TestMethod]
    public async Task CreateHotel_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new HotelDto
        {
            Address = "asd",
            ManagerId = context.GetBobUserId(),
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/hotels", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/hotels with no name");
    }

    [TestMethod]
    public async Task CreateHotel_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new HotelDto
        {
            Name = "a".PadLeft(121, '0'),
            Address = "asd",
            ManagerId = context.GetBobUserId(),
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/hotels", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/hotels with a name that is too long");
    }

    [TestMethod]
    public async Task CreateHotel_NoAddress_ReturnsError()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var target = await webClient.GetHotel();
        await webClient.AssertLoggedInAsAdmin();
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        var request = new HotelDto
        {
            Name = "asd",
            ManagerId = context.GetBobUserId(),
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/hotels", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling POST /api/hotels with no description");
    }

    [TestMethod]
    public async Task CreateHotel_Returns201AndData()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsAdmin();
        var request = new HotelDto
        {
            Name = "a",
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/hotels", request);

        //assert
        await httpResponse.AssertCreateHotelFunctions(request, webClient);
    }

    [TestMethod]
    public async Task CreateHotel_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Name = "a",
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/hotels", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling POST /api/hotels when not logged in");
    }

    [TestMethod]
    public async Task CreateHotel_LoggedInAsBob_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        await webClient.AssertLoggedInAsBob();
        var request = new HotelDto
        {
            Name = "a",
            Address = "asd",
        };

        //act
        var httpResponse = await webClient.PostAsJsonAsync("/api/hotels", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling POST /api/hotels when logged in as bob");
    }

    [TestMethod]
    public async Task UpdateHotel_NoName_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/hotels/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/hotels/{id} with a missing name");
    }

    [TestMethod]
    public async Task UpdateHotel_NameTooLong_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Name = "a".PadLeft(121, '0');

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/hotels/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/hotels/{id} with a name that is too long");
    }

    [TestMethod]
    public async Task UpdateHotel_NoAddress_Returns400()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = null;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/hotels/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "we expect an HTTP 400 when calling PUT /api/hotels/{id} with a missing description");
    }

    [TestMethod]
    public async Task UpdateHotel_Valid_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var sueId = context.GetSueUserId();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        await webClient.AssertLoggedInAsAdmin();
        request.Address = "cool new address";
        request.ManagerId = sueId;

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/hotels/{request.Id}", request);

        //assert
        await httpResponse.AssertHotelUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateHotel_NotLoggedIn_Returns401()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/hotels/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "we expect an HTTP 401 when calling PUT /api/hotels/{id} without being logged in");
    }

    [TestMethod]
    public async Task UpdateHotel_LoggedInAsBob_Returns200()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsBob();

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/hotels/{request.Id}", request);

        //assert
        await httpResponse.AssertHotelUpdateFunctions(request, webClient);
    }

    [TestMethod]
    public async Task UpdateHotel_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        request.Address = "cool new address";

        //act
        var httpResponse = await webClient.PutAsJsonAsync($"/api/hotels/{request.Id}", request);

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling PUT /api/hotels/{id} against a hotel bob manages while logged in as sue");
    }

    [TestMethod]
    public async Task DeleteHotel_NoSuchItem_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Address = "asd",
            Name = "asd"
        };
        await using var itemHandle = await webClient.CreateHotel(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/hotels/{request.Id + 21}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/hotels/{id} with an invalid Id");
    }

    [TestMethod]
    public async Task DeleteHotel_ValidItem_ReturnsOk()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Address = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateHotel(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        var httpResponse = await webClient.DeleteAsync($"/api/hotels/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling DELETE /api/hotels/{id} with a valid id");
    }

    [TestMethod]
    public async Task DeleteHotel_SameItemTwice_ReturnsNotFound()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var request = new HotelDto
        {
            Address = "asd",
            Name = "asd",
        };
        await using var itemHandle = await webClient.CreateHotel(request);
        if (itemHandle == null)
        {
            Assert.Fail("You are not ready for this test");
            return;
        }
        await webClient.AssertLoggedInAsAdmin();

        //act
        await webClient.DeleteAsync($"/api/hotels/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/hotels/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "we expect an HTTP 404 when calling DELETE /api/hotels/{id} on the same item twice");
    }

    [TestMethod]
    public async Task DeleteHotel_LoggedInAsWrongUser_Returns403()
    {
        //arrange
        var webClient = context.GetStandardWebClient();
        var bobId = context.GetBobUserId();
        var request = new HotelDto
        {
            Name = "a",
            Address = "desc",
            ManagerId = bobId,
        };
        await using var target = await webClient.CreateHotel(request);
        if (target == null)
        {
            Assert.Fail("You are not ready for this test");
        }
        await webClient.AssertLoggedInAsSue();

        //act
        await webClient.DeleteAsync($"/api/hotels/{request.Id}");
        var httpResponse = await webClient.DeleteAsync($"/api/hotels/{request.Id}");

        //assert
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden, "we expect an HTTP 403 when calling DELETE /api/hotels/{id} against a hotel bob manages while logged in as sue");
    }
}
