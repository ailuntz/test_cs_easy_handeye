cmake_minimum_required (VERSION 3.10)

if(POLICY CMP0054)
    cmake_policy(SET CMP0054 NEW)
endif()

project (MaintenanceCommandsExample_CSharp LANGUAGES CSharp CXX)

set(Files
    ${MaintenanceCommandsExample_CSharp_SOURCE_DIR}/Program.cs
    ${MaintenanceCommandsExample_CSharp_SOURCE_DIR}/ReadMe.txt
)

add_executable(MaintenanceCommandsExample_CSharp
    ${Files}
)

if(NOT PHO_API_CMAKE_CONFIG_PATH)
    set(PHO_API_CMAKE_CONFIG_PATH "$ENV{PHOXI_CONTROL_PATH}")
endif()

find_package(PhoXi REQUIRED CONFIG PATHS "${PHO_API_CMAKE_CONFIG_PATH}")
        
if(NOT PHO_BUILT_IN_API_IN_EXAMPLES)
    target_link_libraries(MaintenanceCommandsExample_CSharp 
        ${PHOXI_API_MANAGED_DLL_RELEASE_PER_COMPILER})
    
    set_property(TARGET MaintenanceCommandsExample_CSharp PROPERTY 
        VS_DOTNET_REFERENCE_WrapperCSharp ${PHOXI_API_MANAGED_DLL_RELEASE_PER_COMPILER})
endif()

# Set the .NET Framework version for the executable.
set_property(TARGET MaintenanceCommandsExample_CSharp PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.0")
# Set the executable to be 32-bit.
set_property(TARGET MaintenanceCommandsExample_CSharp PROPERTY WIN32_EXECUTABLE FALSE)
# Set the C# language version (defaults to 3.0).
set(CMAKE_CSharp_FLAGS "/langversion:latest")
