cmake_minimum_required (VERSION 3.10)

if(POLICY CMP0054)
    cmake_policy(SET CMP0054 NEW)
endif()

project (ChangeColorSettingsExample_CSharp LANGUAGES CSharp CXX)

set(Files
    ${ChangeColorSettingsExample_CSharp_SOURCE_DIR}/Program.cs
    ${ChangeColorSettingsExample_CSharp_SOURCE_DIR}/ReadMe.txt
)

add_executable(ChangeColorSettingsExample_CSharp
    ${Files}
)

if(NOT PHO_API_CMAKE_CONFIG_PATH)
    set(PHO_API_CMAKE_CONFIG_PATH "$ENV{PHOXI_CONTROL_PATH}")
endif()

find_package(PhoXi REQUIRED CONFIG PATHS "${PHO_API_CMAKE_CONFIG_PATH}")
        
if(NOT PHO_BUILT_IN_API_IN_EXAMPLES)
    target_link_libraries(ChangeColorSettingsExample_CSharp 
        ${PHOXI_API_MANAGED_DLL_RELEASE_PER_COMPILER})
    
    set_property(TARGET ChangeColorSettingsExample_CSharp PROPERTY 
        VS_DOTNET_REFERENCE_WrapperCSharp ${PHOXI_API_MANAGED_DLL_RELEASE_PER_COMPILER})
endif()

# Set the .NET Framework version for the executable.
set_property(TARGET ChangeColorSettingsExample_CSharp PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v4.0")
# Set the executable to be 32-bit.
set_property(TARGET ChangeColorSettingsExample_CSharp PROPERTY WIN32_EXECUTABLE FALSE)
# Add assembly reference for SaveTexture
set_property(TARGET ChangeColorSettingsExample_CSharp PROPERTY VS_DOTNET_REFERENCES "System.Drawing")
# Allow unsafe operations in SaveTexture example
target_compile_options(ChangeColorSettingsExample_CSharp PUBLIC "/unsafe")
# Set the C# language version (defaults to 3.0).
set(CMAKE_CSharp_FLAGS "/langversion:latest")