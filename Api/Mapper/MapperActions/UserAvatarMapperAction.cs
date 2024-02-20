using Api.Models.User;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperActions;

public class UserAvatarMapperAction : IMappingAction<User, UserAvatarModel>
{
	private readonly LinkGeneratorService links;
	public UserAvatarMapperAction(LinkGeneratorService linkGeneratorService)
	{
		links = linkGeneratorService;
	}
	public void Process(User source, UserAvatarModel destination, ResolutionContext context) =>
		links.FixAvatar(source, destination);
}