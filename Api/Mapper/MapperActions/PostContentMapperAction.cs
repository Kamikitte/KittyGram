using Api.Models.Attach;
using Api.Services;
using AutoMapper;
using DAL.Entities;

namespace Api.Mapper.MapperActions;

public class PostContentMapperAction : IMappingAction<PostContent, AttachExternalModel>
{
    private readonly LinkGeneratorService links;

    public PostContentMapperAction(LinkGeneratorService linkGeneratorService)
    {
        links = linkGeneratorService;
    }

    public void Process(PostContent source, AttachExternalModel destination, ResolutionContext context) =>
        links.FixContent(source, destination);
}