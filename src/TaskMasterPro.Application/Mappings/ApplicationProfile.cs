using AutoMapper;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Enums;
using TaskMasterPro.Application.DTOs;

namespace TaskMasterPro.Application.Mappings;

public class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<Board, BoardSummaryDto>();
        CreateMap<Board, BoardDto>();
        CreateMap<Board, BoardDetailDto>();

        CreateMap<BoardColumn, ColumnDto>();
        CreateMap<BoardColumn, ColumnWithTasksDto>();

        // Priority enum to string mapping, and comment count projection
        CreateMap<TaskItem, TaskDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.ColumnId, opt => opt.MapFrom(src => src.BoardColumnId))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<TaskItem, TaskDetailDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.ColumnId, opt => opt.MapFrom(src => src.BoardColumnId))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));
    }
}
