using System;
using System.Linq;

namespace Fortexx.Services {

    public interface IDtoService<TModel, TDto> {
        public TDto GetDto(TModel model);
        public TModel GetModel(TDto dto);
    }

}

namespace Microsoft.Extensions.DependencyInjection {
    
    using Fortexx.Services;
    public static class IDtoServiceExtension {
        public static IServiceCollection AddDataDransferObjectServices(this IServiceCollection services) {
            System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(item => item.GetInterfaces()
            .Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == typeof(IDtoService<,>)) && !item.IsAbstract && !item.IsInterface)
            .ToList()
            .ForEach(assignedTypes =>
            {
                var serviceType = assignedTypes.GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IDtoService<,>));
                services.AddTransient(serviceType, assignedTypes);
            });
            return services;
        }
    }
}