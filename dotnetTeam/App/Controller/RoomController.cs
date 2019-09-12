﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Controller
{
    public class RoomController : Microsoft.AspNetCore.Mvc.Controller
    {
	    private readonly IRoomRepository _roomRepository;
	    private readonly IEventsPublisher _publisher;

	    public RoomController(IEventsPublisher publisher, IRoomRepository roomRepository )
	    {
		    _roomRepository = roomRepository ?? throw new ArgumentNullException(nameof(roomRepository));
		    _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
	    }

        public ActionResult Index()
        {
            return View();
        }

        [Route("[controller]/checkin")]
        public ActionResult CheckIn() 
        {
	        return View();
        }

        [HttpPost]
        [Route("[controller]/checkin")]
        public async Task<ActionResult> CheckIn(string roomId)
        {
	        await _publisher.Publish(new RoomCheckedIn(new RoomId(roomId)));
	        return View();
        }

		[Route("[controller]/tobechecked")]
	    public async Task<ActionResult> ToBeChecked() 
		{
			var model = new ToBeCheckedModel(await _roomRepository.GetCheckedRoomIds(), await _roomRepository.GetNotCheckedRoomIds());
		    return View(model);
	    }

	    [Route("[controller]/checking/{roomId}")]
		public ActionResult Checking(int roomId)
	    {
		    return View(new CheckingModel(roomId));
	    }

	    [Route("[controller]/checkingdone/{roomId}")]
	    public ActionResult CheckingDone(int roomId) 
		{
		    var room = new Room(new RoomId(roomId.ToString()));
		    room.CheckingDone(_publisher);
			return RedirectToAction("tobechecked");
	    }

		[HttpPost]
	    [Route("[controller]/reportdamage/{roomId}")]
	    public ActionResult ReportDamage(int roomId, string description)
		{
		    var room = new Room(new RoomId(roomId.ToString()));
		    room.ReportDamage(_publisher, description);
			return RedirectToAction("tobechecked");
		}

	    [Route("[controller]/checkoutguest/{roomId}")]
		public ActionResult CheckoutGuest(int roomId)
	    {
		    var guest = new Guest();
		    guest.Checkout(_publisher, new RoomId(roomId.ToString()));
			return RedirectToAction("tobechecked");
		}
	}

    public class CheckingModel
	{
		public int RoomId { get; }

		public CheckingModel(int roomId)
		{
			RoomId = roomId;
		}
	}

	public class ToBeCheckedModel
	{
		public RoomId[] CheckedRoomIds { get; }
		public RoomId[] NotCheckedRoomIds { get; }

		public ToBeCheckedModel(RoomId[] checkedRoomIds, RoomId[] notCheckedRoomIds)
		{
			CheckedRoomIds = checkedRoomIds;
			NotCheckedRoomIds = notCheckedRoomIds;
		}
	}
}