# Facebook Group Feeds Processor
An Azure Function scheduler to retrieve feeds from certain Facebook Groups.

Currently, it is running on Azure Functions to retrieve feeds from the following Facebook Groups.

1. [.NET Developers Community Singapore](https://www.facebook.com/groups/sg.netdev)
2. [Azure Community Singapore](https://www.facebook.com/groups/azure.community.singapore)

The two groups are maintained by same group of passionate .NET developers. So, sometimes two groups will share the same feeds. In this feeds processor, similar posts with the same URL will be taken as only one.

The result of it can been seen at the homepage of [dotnet.sg](http://dotnet.sg "Singapore .NET Developers Community") under the section "Latest Topic on Our Facebook".