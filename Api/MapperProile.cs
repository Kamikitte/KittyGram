using AutoMapper;
using Common;

namespace Api
{
	public class MapperProile : Profile
	{
		public MapperProile()
		{
			CreateMap<Models.CreateUserModel, DAL.Entities.User>()
				.ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
				.ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
				.ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime));

			CreateMap<DAL.Entities.User, Models.UserModel>();

			CreateMap<DAL.Entities.Attach, Models.AttachModel>();

			CreateMap<DAL.Entities.Avatar, Models.AttachModel>();

			CreateMap<DAL.Entities.Post, Models.PostModel>();

			CreateMap<DAL.Entities.PostAttach, Models.AttachModel>();
		}
	}
}
