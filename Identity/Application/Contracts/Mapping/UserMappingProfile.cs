using AutoMapper;
using Common.Contracts.DTO.Request.User;
using Identity.Application.Contracts.DTO.Response.User;
using Identity.Application.Contracts.Enum;
using Identity.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Contracts.Mapping
{
    public class UserMappingProfile : Profile
    {

        public UserMappingProfile()
        {
            // RegisterRequest → AppUser
            CreateMap<RegisterRequest, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.UserName) ? src.Email : src.UserName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.DOB, opt => opt.MapFrom(src => src.DOB))
                .ForMember(dest => dest.Gender,
                 opt => opt.MapFrom(src => MapStringToGender(src.Gender) ?? GenderType.Unknown))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore());

            // AppUser → UserResponse
            CreateMap<AppUser, UserResponse>()
                //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DOB))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                 .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => MapGenderToString(src.Gender)))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.ProfilePictureUrl))
                                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))

                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            // UpdateUserRequest → AppUser
            //CreateMap<UpdateUserRequest, AppUser>()
            //  .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            //  .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            //  .ForMember(dest => dest.DOB, opt => opt.MapFrom(src => src.DOB))
            //  .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
            //  .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => MapStringToGender(src.Gender)))
            //  .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            //  .ForAllMembers(opt => opt.Condition(
            //      (src, dest, srcMember) => srcMember != null));
            //CreateMap<UpdateUserRequest, AppUser>()
            //   // تعيين الحقول الأساسية مع التحقق من القيم
            //   .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src =>
            //       !string.IsNullOrWhiteSpace(src.FirstName) ? src.FirstName : null))
            //   .ForMember(dest => dest.LastName, opt => opt.MapFrom(src =>
            //       !string.IsNullOrWhiteSpace(src.LastName) ? src.LastName : null))
            //   .ForMember(dest => dest.DOB, opt => opt.MapFrom(src =>
            //       src.DOB.HasValue ? src.DOB : null))
            //   .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src =>
            //       !string.IsNullOrWhiteSpace(src.Phone) ? src.Phone : null))
            //   .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src =>
            //       !string.IsNullOrWhiteSpace(src.ProfilePictureUrl) ? src.ProfilePictureUrl : null))
            //   .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
            //       !string.IsNullOrWhiteSpace(src.Address) ? src.Address : null))

            //   // معالجة خاصة لحقل الجنس (تحويل من string إلى enum)
            //   .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
            //       !string.IsNullOrWhiteSpace(src.Gender) ?
            //       MapStringToGender(src.Gender) : GenderType.Unknown))

            //   // تحديث تاريخ التعديل تلقائياً
            //   .AfterMap((src, dest) =>
            //   {
            //       dest.UpdatedAt = DateTime.UtcNow;
            //   });

            CreateMap<UpdateUserRequest, AppUser>()
                .ForMember(dest => dest.FirstName, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.FirstName)))
                .ForMember(dest => dest.LastName, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.LastName)))
                .ForMember(dest => dest.DOB, opt => opt.Condition(src => src.DOB.HasValue))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Phone)))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.ProfilePictureUrl)))
                .ForMember(dest => dest.Address, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Address)))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.Gender)
                        ? MapStringToGender(src.Gender)
                        : GenderType.Unknown))

            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                srcMember switch
                {
                    string str => !string.IsNullOrWhiteSpace(str),
                    _ => srcMember != null
                }));



            CreateMap<AppUser, UpdateUserRequest>()
          .ForMember(dest => dest.Gender,
                     opt => opt.MapFrom(src => src.Gender.ToString()))
          .ForMember(dest => dest.CurrentPassword,
                     opt => opt.Ignore());

            //    CreateMap<UpdateUserRequest, AppUser>()
            // // خصائص فردية مع شروط التعيين
            // .ForMember(dest => dest.FirstName, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.FirstName)))
            // .ForMember(dest => dest.LastName, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.LastName)))
            // .ForMember(dest => dest.DOB, opt => opt.Condition(src => src.DOB.HasValue))
            // .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Phone)))
            // .ForMember(dest => dest.ProfilePictureUrl, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.ProfilePictureUrl)))
            // .ForMember(dest => dest.Address, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Address)))
            // .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>
            //     !string.IsNullOrWhiteSpace(src.Gender)
            //         ? MapStringToGender(src.Gender)
            //         : GenderType.Unknown))

            // // استثناء UpdatedAt من جميع الشروط العامة
            // .ForAllPropertyMaps(
            //     pm => pm.DestinationName != nameof(AppUser.UpdatedAt),
            //     (pm, opt) =>
            //     {
            //         opt.Condition((src, dest, srcMember) =>
            //             srcMember switch
            //             {
            //                 string str => !string.IsNullOrWhiteSpace(str),
            //                 _ => srcMember != null
            //             });
            //     });
        }
        private static string MapGenderToString(GenderType gender)
        {
            return gender switch
            {
                GenderType.Male => "Male",
                GenderType.Female => "Female",
                GenderType.Other => "Other",
                GenderType.PreferNotToSay => "Prefer not to say",
                _ => "Unknown"
            };
        }
        private static GenderType? MapStringToGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return null;

            return gender.Trim().ToLowerInvariant() switch
            {
                "male" => GenderType.Male,
                "female" => GenderType.Female,
                "other" => GenderType.Other,
                "prefer not to say" => GenderType.PreferNotToSay,
                _ => GenderType.Unknown
            };
        }
    }
}
