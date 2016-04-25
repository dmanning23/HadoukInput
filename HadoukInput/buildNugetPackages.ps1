nuget pack .\HadoukInput.nuspec -IncludeReferencedProjects -Prop Configuration=Release
nuget push *.nupkg