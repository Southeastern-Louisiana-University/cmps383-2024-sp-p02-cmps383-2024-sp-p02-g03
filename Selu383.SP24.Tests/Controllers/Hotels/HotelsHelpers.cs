using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Selu383.SP24.Tests.Controllers.Authentication;
using Selu383.SP24.Tests.Dtos;
using Selu383.SP24.Tests.Helpers;

namespace Selu383.SP24.Tests.Controllers.Hotels;

internal static class HotelsHelpers
{
    internal static async Task<IAsyncDisposable?> CreateHotel(this HttpClient webClient, HotelDto request)
    {
        try
        {
            await webClient.AssertLoggedInAsAdmin();
            var httpResponse = await webClient.PostAsJsonAsync("/api/hotels", request);
            var resultDto = await AssertCreateHotelFunctions(httpResponse, request, webClient);
            await webClient.AssertLoggedOut();
            request.Id = resultDto.Id;
            return new DeleteHotel(resultDto, webClient);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            return null;
        }
    }

    internal static async Task<HotelDto?> GetHotel(this HttpClient webClient)
    {
        try
        {
            var getAllRequest = await webClient.GetAsync("/api/hotels");
            var getAllResult = await AssertHotelListAllFunctions(getAllRequest);
            return getAllResult.OrderByDescending(x => x.Id).First();
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            return null;
        }
    }

    internal static async Task AssertHotelUpdateFunctions(this HttpResponseMessage httpResponse, HotelDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling PUT /api/hotels/{id} with valid data to update a hotel");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<HotelDto>();
        resultDto.Should().BeEquivalentTo(request, "We expect the update hotel endpoint to return the result");

        var getByIdResult = await webClient.GetAsync($"/api/hotels/{request.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the updated hotel by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<HotelDto>();
        dtoById.Should().BeEquivalentTo(request, "we expect the same result to be returned by an update hotel call as what you'd get from get hotel by id");

        var getAllRequest = await webClient.GetAsync("/api/hotels");
        var listAllData =  await AssertHotelListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/hotels");
        listAllData.Should().NotBeEmpty("list all should have something if we just updated a hotel");
        var matchingItem = listAllData.Where(x => x.Id == request.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created hotel by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(request, "we expect the same result to be returned by a updated hotel as what you'd get from get getting all hotels");
    }

    internal static async Task<HotelDto> AssertCreateHotelFunctions(this HttpResponseMessage httpResponse, HotelDto request, HttpClient webClient)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created, "we expect an HTTP 201 when calling POST /api/hotels with valid data to create a new hotel");

        var resultDto = await httpResponse.Content.ReadAsJsonAsync<HotelDto>();
        Assert.IsNotNull(resultDto, "We expect json data when calling POST /api/hotels");

        resultDto.Id.Should().BeGreaterOrEqualTo(1, "we expect a newly created hotel to return with a positive Id");
        resultDto.Should().BeEquivalentTo(request, x => x.Excluding(y => y.Id), "We expect the create hotel endpoint to return the result");

        httpResponse.Headers.Location.Should().NotBeNull("we expect the 'location' header to be set as part of a HTTP 201");
        httpResponse.Headers.Location.Should().Be($"http://localhost/api/hotels/{resultDto.Id}", "we expect the location header to point to the get hotel by id endpoint");

        var getByIdResult = await webClient.GetAsync($"/api/hotels/{resultDto.Id}");
        getByIdResult.StatusCode.Should().Be(HttpStatusCode.OK, "we should be able to get the newly created hotel by id");
        var dtoById = await getByIdResult.Content.ReadAsJsonAsync<HotelDto>();
        dtoById.Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a create hotel as what you'd get from get hotel by id");

        var getAllRequest = await webClient.GetAsync("/api/hotels");
        var listAllData =  await AssertHotelListAllFunctions(getAllRequest);

        Assert.IsNotNull(listAllData, "We expect json data when calling GET /api/hotels");
        listAllData.Should().NotBeEmpty("list all should have something if we just created a hotel");
        var matchingItem = listAllData.Where(x => x.Id == resultDto.Id).ToArray();
        matchingItem.Should().HaveCount(1, "we should be a be able to find the newly created hotel by id in the list all endpoint");
        matchingItem[0].Should().BeEquivalentTo(resultDto, "we expect the same result to be returned by a created hotel as what you'd get from get getting all hotels");

        return resultDto;
    }

    internal static async Task<List<HotelDto>> AssertHotelListAllFunctions(this HttpResponseMessage httpResponse)
    {
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK, "we expect an HTTP 200 when calling GET /api/hotels");
        var resultDto = await httpResponse.Content.ReadAsJsonAsync<List<HotelDto>>();
        Assert.IsNotNull(resultDto, "We expect json data when calling GET /api/hotels");
        resultDto.Should().HaveCountGreaterThan(2, "we expect at least 3 hotels when calling GET /api/hotels");
        resultDto.All(x => !string.IsNullOrWhiteSpace(x.Name)).Should().BeTrue("we expect all hotels to have names");
        resultDto.All(x => x.Id > 0).Should().BeTrue("we expect all hotels to have an id");
        var ids = resultDto.Select(x => x.Id).ToArray();
        ids.Should().HaveSameCount(ids.Distinct(), "we expect Id values to be unique for every hotel");
        return resultDto;
    }

    private sealed class DeleteHotel : IAsyncDisposable
    {
        private readonly HotelDto request;
        private readonly HttpClient webClient;

        public DeleteHotel(HotelDto request, HttpClient webClient)
        {
            this.request = request;
            this.webClient = webClient;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await webClient.AssertLoggedInAsAdmin();
            }
            catch (Exception)
            {
                // ignored
            }
            try
            {
                await webClient.DeleteAsync($"/api/hotels/{request.Id}");

            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
