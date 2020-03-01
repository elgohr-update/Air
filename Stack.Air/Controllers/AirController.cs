using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using com.b_velop.Stack.Air.Data;
using com.b_velop.Stack.Air.Data.Dtos;
using com.b_velop.Stack.Air.Data.Enums;
using com.b_velop.Stack.Air.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Stack.Air.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirController : ControllerBase
    {
        private IMapper _mapper;
        private DataContext _context;

        public AirController(
            DataContext context,
            IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAirAsync()
        {
            var cultureInfo = new CultureInfo("de-DE");
            var airs = await _context.Times.Include("Values").Include("Values.Sensor").ToListAsync();
            var result = new List<ValuesForListDto>();
            foreach (var air in airs)
            {
                result.Add(new ValuesForListDto
                {
                    Humidity = air.Values.First(_ => _.Sensor.Id == (long)SensorType.DhtHumidity).MeasureValue,
                    Temperature = air.Values.First(_ => _.Sensor.Id == (long)SensorType.DhtTemperature).MeasureValue,
                    Pressure = air.Values.First(_ => _.Sensor.Id == (long)SensorType.BmpPressure).MeasureValue,
                    SdsP1 = air.Values.First(_ => _.Sensor.Id == (long)SensorType.SdsP1).MeasureValue,
                    SdsP2 = air.Values.First(_ => _.Sensor.Id == (long)SensorType.SdsP2).MeasureValue,
                    Time = air.Timestamp.ToString(cultureInfo)
                });
            }
            return Ok(result);
        }

        [HttpGet("timestamp/{id}", Name = "GetTimes")]
        public async Task<IActionResult> GetTimestampByIdAsync(
            long id)
        {
            var timestamp = await _context.Times
                .Include("Values")
                .Include("Values.Sensor")
                .FirstOrDefaultAsync(_ => _.Id == id);
            var timestampDto = _mapper.Map<TimeDto>(timestamp);
            return Ok(timestampDto);
        }

        [HttpPost]
        public async Task<IActionResult> PostAirAsync(
            AirFromSensorDto airFromSensorDto)
        {
            if(airFromSensorDto.Name != "2063272")
            {
                return new UnauthorizedResult();
            }

            var time = new Time
            {
                Timestamp = airFromSensorDto.Time.UtcDateTime,
                Values = new List<Value>()
            };

            foreach (var value in airFromSensorDto.SensorDataValues)
            {
                var currentValue = _mapper.Map<Value>(value);
                time.Values.Add(currentValue);
            }
            _context.Times.Add(time);
            await _context.SaveChangesAsync();
            var timeDto = _mapper.Map<TimeDto>(time);
            return CreatedAtRoute("GetTimes", new { time.Id }, timeDto);
        }
    }
}