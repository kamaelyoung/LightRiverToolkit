LightRiverToolkit
===================

<br/>
提供一些開發上累積的一些架構經驗的程式碼，希望可以減少開發上的一些功夫 <br/>
目前支援的平台是.NET 4.5.1 & Windows Store 8.1 & Windows Phone 8.1 ，但應該如果要向下支援應該也不是難事<br/>
<br/><br/>

##Solutiou結構說明##
位於Solution的資料夾中，打開後可以看到程式碼結構上有三層

- Portable：跨包含Desktop .NET都可以的library，目前Portable是.NET 4.5.1 & WP8.1 & Win8.1，可以依照自己的需求再去調整
- Platform ：平台限定相關的library，所以看到有分兩個LightRiver.Universal, LightRiver.Desktop，
- App：最上層的App，就是可以執行的東西啦！

<br/>
**下面針對每個Project摘要說明**

##LightRiver project##
###非Net相關的class摘要說明###

 - **EnumDescriptionAttribute**：可以讓你在enum上面定義attribute，然後透過另外的EnumHelper取得Attribute裡面的Key 或 Value
 - **BaseBindable**：實做INotifyPropertyChange，提供SetPrperty的method方便比對後，若有改變會觸發PropertyChange事件
 - **SafeRaise**：安全的觸發事件的class，避免raise condition造成要觸發事件的時候發生exception
 - **Singleton**：目前沒用是被#define掉的，在.NET 4.5的版本之上可以用Lazy取代，反之是可以啟用
 - **ObjectDictionary**：顧名思義是繼承自Dictionary，除此之外提供額外的泛型的GetValue<T>以及TryGetValue<T>的method，並有提供SaveAsync & LoadAsync的method
 - **ObjectEventArgs、StringEventArgs**：自定義事件的時候可以簡單使用的EventArgs
 - **CollectionExtensions**：當你用Dictionary的時候可以有一個AddOrReplace extension method，減少自己判斷的程式碼XD
 - **StringExtensions**：提供SubStringWithDefault的extension method，可以避免發生例外的SubString method
 - **PlatformService**：這個資料夾都是與平台相關的一些Interface定義，留在Platform相關的libray實做
 - **JsonIoUtility**：提供從檔案或Stream序列化、反序列化Utility class，省去一些重複的Code去讀檔的程式碼
 - **AssemblyExtensions**：取得Assembly的一些資訊

###Net相關的class說明###

這部分與網路相關，依照自己的經驗希望能夠簡化一些網路發送/接收（一送一收）的動作，分兩個部分http與socket（http的單純很多）
網路發送接收資料我把他當作做幾個部分

- **Transport**：傳輸層面的，僅有Socket部分會有，因為Socket的收送動作都是自己來的，http的話httpClient已經幫你處理掉這些事情了

> 在Net.Sockets的資料夾下的均是與Transport相關的class，我把Sokcet傳輸的動作分成三個部分Connector（負責連線）, Sender（負責發送封包）, Receiver（負責接收封包）<br/><br/>
> Connector, Sender, Receiver均需要一個ITelegramSocket的interface，因為其實Socket class在Desktop .NET 與 WinRT是不同的，是與平台相關的，所以定義了一個ITelegramSocket的interface交由平台去實做<br/><br/>
> 必須要注意的是Receiver裡面的SocketReceiver的接收部分程式碼是需要您自己去實做，目前會throw NotImplementException，因為Socket的封包怎麼接收牽扯到每個人自己定義的protocol，就麻煩需要的人去實做這段

 - **Parameter**：要組成一個Request的參數
 

> 這邊仿照JSON.NET，我定義了ServiceParameterAttribute與ServiceIgnoreAttribute，讓Service會自動依照你參數的Property組成所需要發送的字串（若Property不指定ServiceParameter的話，就預設以Property名稱組合）<br/><br/>
> IServiceParameterConveter這個interface是ServiceParameterAttribute的一個參數，讓你可以指定自己要組成要發送的字串的時候的Converter

 - **Parser**：解析Response後的資料用的
 

> IParser<TSource>定義一個Parser必要的method，將需要解析資料的來源class定義成泛型，Parse出來的結果是一個object的型態<br/><br/>
> BaseParser<TResult, TSource>：繼承IParser，進一步將Parse出來的結果也定義成ParseResult<TResult>的泛型，可以知道此Parser回傳值的型態，不需要猜測轉型<br/><br/>
> JsonParser：鑑於目前response的字串通常都是JSON string，所以直接繼承BaseParser實做了一個泛型的Json strnig的Parser

 - **Model**：當然是解析完後對應的資料，隨便，就是定義在Parser的TResult
 - **Service**：將一組Parameter, Parser, Model組合在一起的就是一個Service

> 放置於ServiceModel的資料夾下，分為Http與Socket兩種Service，同Service的說明，Service均是由TResult, TParameter, TParser三個部分組成的泛型class<br/><br/>
> 針對BaseHttpService部分，有額外限定TParameter是要繼承HttpServiceParameter，因為HttpServiceParameter裡面有一個Property，可以指定目前的Request是要走GET or POST，GET比較單純，針對POST的要組合的內容目前的實做很簡單

<br/>
##LightRiver.Universal##
針對定義在LightRiver中的幾個Platform相關的Inteface去實做，同時也實做了ITelegramSocket，額外提供了一個StorageHelper做一些WinRT下檔案處理的utility class

<br/>
##LightRiver.Desktop##
針對定義在LightRiver中的幾個Platform相關的Inteface去實做，同時也實做了ITelegramSocket
> LightRiver.Desktop 實做較少，因為目前主要沒有在開發Desktop

<br/>
##LightRiverSample##
展示用的Universal App
> 目前是空的，有空會再補上Sample

<br/>

## 編譯環境需求 ##
Visual Studio 2013 <br/>