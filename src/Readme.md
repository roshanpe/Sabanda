
#Run API# 
dotnet run --project src/Sabanda.API/Sabanda.API.csproj

#Swagrer#
http://127.0.0.1:5142/swagger/index.html


#Run Integration Tests#
dotnet test tests/Sabanda.IntegrationTests/Sabanda.IntegrationTests.csproj --logger "console;verbosity=minimal"

#Writing detailed output to a file#
dotnet test tests/Sabanda.IntegrationTests/Sabanda.IntegrationTests.csproj --logger "console;verbosity=detailed" > full-test-output.txt 2>&1

#Run UI#
cd /media/roshan/Data/git/my/Sabanda/sabanda-web
npm run dev -- --host 0.0.0.0