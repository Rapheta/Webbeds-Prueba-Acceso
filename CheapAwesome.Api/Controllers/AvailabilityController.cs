﻿using AutoMapper;
using CheapAwesome.Core.DTOs;
using CheapAwesome.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CheapAwesome.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]/{destinationId?}/{nights?}")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(IAvailabilityService availabilityService, IMapper mapper, IConfiguration configuration, ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        ///     Retrieve all availabilities
        /// </summary>
        /// <param name="destinationId">Destination number</param>
        /// <param name="nights">Number of nights</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAvailability(int destinationId, int nights)
        {
            var host = _configuration.GetValue<string>("Url:Host");
            var code = _configuration.GetValue<string>("Url:Parameters:code");
            var url = host + "?destinationId=" + destinationId + "&nights=" + nights + "&code=" + code;
            var timeout = _configuration.GetValue<int>("Url:Timeout");

            _logger.LogInformation("Getting availabilities");
            var availability = await _availabilityService.GetAvailability(url, timeout);
            _logger.LogInformation("Got availabilities");

            var availabilityDto = _mapper.Map<IEnumerable<AvailabilityDto>>(availability);

            foreach (var hotel in availabilityDto)
            {
                foreach (var rate in hotel.rates)
                {
                    if (rate.rateType == "PerNight") rate.value = rate.value * Convert.ToInt32(nights);
                }
            }

            return Ok(availabilityDto);
        }
    }
}
