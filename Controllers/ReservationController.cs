using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TODOAPI.Models;
using TODOAPI.Services;


namespace TODOAPI.controllers;


[ApiController]
[Route("[controller]")]
public class HotelController : ControllerBase
{
    private readonly ReservationServices _reservationServices;

    public HotelController(ReservationServices reservationServices)
    {
        _reservationServices = reservationServices;
    }

    [HttpGet]
    public async Task<List<RoomReservation>> GetReservations()
    {
        return await _reservationServices.GetAllAsync();
    }

    [HttpGet("{id}", Name = "GetById")]
    public async Task<RoomReservation> GetReservation(String id)
    {
        return await _reservationServices.GetAsync(id);
    }

    [HttpGet("{name}", Name = "GetByName")]
    public async Task<RoomReservation> GetByName(String guestname)
    {
        return await _reservationServices.GetByNameAsync(guestname);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RoomReservation reservation)
    {


        await _reservationServices.CreateAsync(reservation);
        return CreatedAtAction(nameof(GetReservations), new { id = reservation.Id }, reservation);

        // var token = reservation.Token;
        // if (token == null ){
        //     await _reservationServices.CreateAsync(reservation);
        //     return CreatedAtAction(nameof(GetReservations), new {id = reservation.Id}, reservation);
        // }else {
        //     return BadRequest("token key invalid or empty");
        // }
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] RoomReservation reservation, String id)
    {


        await _reservationServices.UpdateAsync(id, reservation);
        return NoContent();

    }


    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] RoomReservation reservation, String id)
    {

        await _reservationServices.CancelReservationAsync(id);
        return NoContent();

    }
}