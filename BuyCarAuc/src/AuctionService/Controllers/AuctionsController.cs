using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuction()
        {
            var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
            return _mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();
            return _mapper.Map<AuctionDto>(auction);
        }
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            //TODO: tilføj current user som sælger
            auction.Seller = "test";

            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Kunne ikke gemme ændring til DB!");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));

        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null) return NotFound();
            //TODO: tjek sælger == brugernavn
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();
            return BadRequest("Der er problemer med at gemme ændringer");

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            
            if (auction == null) return NotFound();
            //TODO: tjek sælger == brugernavn
            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;
            if (!result) return BadRequest("Kunne ikke opdatere DB");
            return Ok();
        }
    }



}