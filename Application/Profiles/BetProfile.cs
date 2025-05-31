using AutoMapper;
using Application.Models;
using Domain.Entities;

namespace Application.Profiles
{
    public class BetProfile : Profile
    {
        public BetProfile()
        {
            CreateMap<PlaceBetDTO, Bet>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Game, opt => opt.Ignore())
                .ForMember(dest => dest.Payout, opt => opt.Ignore());

            CreateMap<Bet, PlaceBetDTO>();
        }
    }
}
