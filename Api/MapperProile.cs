using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common;

namespace Api
{
    public class MapperProile : Profile
	{
		public MapperProile()
		{
			CreateMap<CreateUserModel, DAL.Entities.User>()
				.ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
				.ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
				.ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime));

			CreateMap<DAL.Entities.User, UserModel>();

			CreateMap<DAL.Entities.User, UserAvatarModel>();

			CreateMap<DAL.Entities.Avatar, AttachModel>();

			CreateMap<DAL.Entities.PostContent, AttachModel>();

			CreateMap<DAL.Entities.PostContent, AttachExternalModel>();

			CreateMap<CreatePostRequest, CreatePostModel>();

			CreateMap<MetadataModel, MetadataLinkModel>();

			CreateMap<MetadataLinkModel, DAL.Entities.PostContent>();

			CreateMap<CreatePostModel, DAL.Entities.Post>()
				.ForMember(d => d.PostContents, m => m.MapFrom(s => s.Contents))
				.ForMember(d => d.CreatingDate, m => m.MapFrom(s => DateTime.UtcNow));

			CreateMap<CreateCommentModel, DAL.Entities.Comment>()
				.ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
				.ForMember(d => d.CreatingDate, m => m.MapFrom(s => DateTime.UtcNow));

			CreateMap<DAL.Entities.Comment, CommentModel>();
		}
	}
}
