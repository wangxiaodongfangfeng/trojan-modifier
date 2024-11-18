# How to build

```
docker build -t trojan-modifier-api .

# how to run docker
docker run --name trojan-modifier-api -p 8080:8080 -p 1080:1080 -p 443:443 trojan-modifier-api
```

## Publish application with trojan copied
Use vim to edit *.csproj 

```
    <Content Include="trojan\trojan">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
```
Publish commands
```aiignore
dotnet publish -c Release -o /app --self-contained true -r linux-x64 /p:PublishSingleFile=true

```