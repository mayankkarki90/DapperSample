using AutoMapper;
using DapperSample.Models;
using DataContracts.Models;

namespace DapperSample
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Employee, EmployeeResponse>().ReverseMap();
        }
    }
}
