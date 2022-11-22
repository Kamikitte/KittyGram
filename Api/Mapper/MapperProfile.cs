using Api.Mapper.MapperActions;
using Api.Models.Attach;
using Api.Models.Comment;
using Api.Models.Like;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common;
using DAL.Entities;

namespace Api.Mapper
{
	public class MapperProfile : Profile
	{
		public MapperProfile()
		{
			CreateMap<CreateUserModel, User>()
				.ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
				.ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
				.ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime));

			CreateMap<User, UserModel>();

			CreateMap<User, UserAvatarModel>()
				.ForMember(d => d.PostsCount, m => m.MapFrom(s => s.Posts!.Count))
				.AfterMap<UserAvatarMapperAction>();

			CreateMap<Avatar, AttachModel>();

			CreateMap<PostContent, AttachModel>();

			CreateMap<PostContent, AttachExternalModel>().AfterMap<PostContentMapperAction>();

			CreateMap<Post, PostModel>()
				.ForMember(d => d.Contents, m => m.MapFrom(d => d.PostContents))
				.ForMember(d => d.LikesCount, m => m.MapFrom(s => s.Likes!.Count));

			CreateMap<CreatePostRequest, CreatePostModel>();

			CreateMap<MetadataModel, MetadataLinkModel>();

			CreateMap<MetadataLinkModel, PostContent>();

			CreateMap<CreatePostModel, Post>()
				.ForMember(d => d.PostContents, m => m.MapFrom(s => s.Contents))
				.ForMember(d => d.CreatingDate, m => m.MapFrom(s => DateTime.UtcNow));

			CreateMap<CreateCommentModel, Comment>()
				.ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
				.ForMember(d => d.CreatingDate, m => m.MapFrom(s => DateTime.UtcNow));

			CreateMap<Comment, CommentModel>()
				.ForMember(d => d.LikesCount, m => m.MapFrom(s => s.Likes!.Count));

			CreateMap<LikePostRequestModel, LikePost>()
				.ForMember(d => d.CreateTime, m => m.MapFrom(s => DateTime.UtcNow));

			CreateMap<LikeCommentRequestModel, LikeComment>()
				.ForMember(d => d.CreateTime, m => m.MapFrom(s => DateTime.UtcNow));
		}
	}
}
