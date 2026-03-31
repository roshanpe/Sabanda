
#Run API# 
dotnet run --project src/Sabanda.API/Sabanda.API.csproj

#Swagrer
http://127.0.0.1:5142/swagger/index.html


#Run Integration Tests#
dotnet test tests/Sabanda.IntegrationTests/Sabanda.IntegrationTests.csproj --logger "console;verbosity=minimal"

