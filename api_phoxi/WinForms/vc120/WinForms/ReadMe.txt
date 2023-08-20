========================================================================
    WINDOWS FORMS APPLICATION : WinFormsNoCMake Project Overview
========================================================================

This is a simple example of how to use Photoneo PhoXi C# API without CMake in
Windows Forms application.

You will learn how to:

* set up your C# project independent from CMake,
* use WinForms in your project.

==============
Prerequisites:
==============
Copy following libraries:
- For MSVC 12
    - PhoXi_API_msvc12_Debug.dll
    - PhoXi_API_msvc12_Release.dll
    - WrapperCSharp_msvc12_Debug.dll
    - WrapperCSharp_msvc12_Release.dll
- For MSVC 14
    - PhoXi_API_msvc14_Debug.dll
    - PhoXi_API_msvc14_Release.dll
    - WrapperCSharp_msvc14_Debug.dll
    - WrapperCSharp_msvc14_Release.dll
From %PHOXI_CONTROL_PATH%\API\bin
To WinFormsNoCmake folder.

=========
Overview:
=========
No CMake
Example application shows how to use Wrapper C# API without CMake.
Please open WinFormsNoCMake.csproj in your text editor for details.
To use Release Wrapper C# API dll in Release configuration and
Debug Wrapper C# API dll in Debug configuration we wrap our Reference in
ItemGroup with a Condition like so:
<ItemGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <Reference Include="WrapperCSharp_${COMPILER_VERSION}_Debug_${PHO_SOFTWARE_VERSION}.dll">
        <HintPath>WrapperCSharp_${COMPILER_VERSION}_Debug_${PHO_SOFTWARE_VERSION}.dll</HintPath>
    </Reference>
</ItemGroup>

WinForms and async/await
This example also illustrates how to use Wrapper C# API dlls in WinForms
application without blocking the UI thread during API calls.

/////////////////////////////////////////////////////////////////////////////
