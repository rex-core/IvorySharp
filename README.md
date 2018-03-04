# IvorySharp
> Поддерживаемые платформы: .NET Framework 4.6.1, .NET Core 2.0


Библиотека предоставляет набор компонентов, реализующих некоторые возможности парадигмы [АОП](https://ru.wikipedia.org/wiki/Аспектно-ориентированное_программирование) в языке C#. Если точнее, то библиотека позволяет вынести сквозную функциональность в отдельные компоненты и декларативно применять их к методам бизнес логики, используя атрибуты.


### И зачем мне это ваше АОП ?
Как уже отмечалось, применений идей парадигмы позволяет избавиться от дублирующегося кода, разбросанного по всем слоям приложения. Примером может стать обработка ошибок. Допустим, есть ряд репозиториев, расположенных на уровне доступа к данным.
```C#
interface IUserRepository 
{
    int Add(User user);
    int Update(User user); 
}

```
В каждом методе репозитория находится одинаковая обработка ошибок вида
```C#
class UserRepository : IUserRepository 
{
  int Add(User user)
  {
    try {
      // Добавление в базу пользователя
    } catch (Exception e) {
      Logger.Error(e.Message, e);
      throw new WrappedException(e);
    }
  }
}

```
Рано или поздно обилие однообразных фрагментов кода приводит к появлению обобщенных обработчиков вида
```C#
class ExceptionHandlers {

  public static T HandleExceptions<T>(Func<T> action) 
  { ... }

}
```
И репозиторий преображается в следующий вид
```C#
class UserRepository : IUserRepository 
{
  int Add(User user)
  {
    return ExceptionHandlers.HandleExceptions(() => 
    { 
      // Добавление пользователя
    });
  }
}
```
Однако, это не решает проблему и так же нарушает принцип [DRY](https://ru.wikipedia.org/wiki/Don’t_repeat_yourself), так как для отличающихся по сигнатуре делегатов придется писать свой дублирующийся фрагмент кода. При этом комбинирование такого рода обработчиков сводит читаемость и удобство поддержки функциональности на нет. В варианте с применением АОП репозиторий преобразуется следующим образом

```C#
interface IUserRepository 
{
    [HandleExceptionAspect]
    int Add(User user);
}

class UserRepository : IUserRepository {
  int Add(User user) {
    // Добавление пользователя
  }
}

[AttributeUsage(AttributeTargets.Method)]
class HandleExceptionAspect : MethodBoundaryAspect 
{
  public override void OnException(IInvocationPipeline pipeline)
  {
    var exception = pipeline.CurrentException;
    Logger.Error(exception.Message, exception);
    
    // Выбросит исключение, сохранив стек-трейс
    pipeline.Throw(new WrapperException(exception);
  }
}

```   
При этом возможно применение нескольких аспектов, в том числе аспектов верхнего уровня на интерфейсах без потери читаемости и поддерживаемости бизнес логики. Детализированные примеры аспектов и их использования представлены [тут](https://github.com/rex-core/IvorySharp/tree/master/IvorySharp.Examples).

### И никто раньше такого не делал?
Делали само собой. Наиболее популярная библиотека реализующая АОП  - это [PostSharp](https://www.postsharp.net). Его функциональность гораздо шире этой библиотеки, однако есть один минус - он платный. Есть и бесплатные альтернативы, например [Fody](https://github.com/Fody/Fody) и [Mr.Advice](https://github.com/ArxOne/MrAdvice), однако у них другой принцип работы. Данная библиотека выполняет динамическое проксирование вызовов через стандартные механизмы платформы ([RealProxy](https://msdn.microsoft.com/ru-ru/library/system.runtime.remoting.proxies.realproxy(v=vs.110).aspx), [DispatchProxy](https://github.com/dotnet/corefx/blob/master/src/System.Reflection.DispatchProxy/src/System/Reflection/DispatchProxy.cs)), в то время как PostSharp и Fody выполняют [инъекцию IL кода](http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/) во время компиляции, что улучшает общую производительность, однако модифицирует код сборки. Наиболее близкое по принципу работы к представленной библиотеке - механизм динамического перехвата вызова в [Castle.DynamicProxy](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy-introduction.md).
Для того чтобы лучше понять детали реализации библиотеки советую ознакомиться с [этой](https://msdn.microsoft.com/en-us/magazine/dn574804.aspx) статьей.

## Ограничения
* Аспекты можно использовать только на интерфейсы и методы интерфейсов, **на классах, свойствах, конструкторах и прочем - работать не будет**;
* Дублирующиеся аспекты удаляются. То есть, если на интерфейсе есть аспект ```[FooAspect]``` и на каком-то методе интерфейса есть такой же аспект, то будет примененен только аспект верхнего уровня;
* С асинхронными методами нормально не работает. Доработки в планах;
* Ответственность за исключения внутри точек внедрения в вызов оригинального метода на разработчике. Если произойдет исключение в каком-то из обработчиков (OnEntry, OnSuccess, etc), то вызов основного метода будет прерван, а возникшее исключение попадет в вызывающий код;
* Влияет на перфоманс. Динамическое проксирование определенно не самая быстрая штука. Детальное описание с циферками будет позже.

## Виды аспектов
1. [**MethodBoundaryAspect**](https://github.com/rex-core/IvorySharp/blob/master/IvorySharp/Aspects/IMethodBoundaryAspect.cs). Выполняет переопределенные методы в точках перед и после фактического вызова основного метода.
Развернутый вид метода, на который применен аспект данного вида можно представить следующим образом.
```C#
interface Foo 
{
  [MyAspect]
  void Bar();
}

// Вызов new FooImpl().Bar(); преобразуется в

try {
  MyAspect.OnEntry(pipeline);
  Bar();
  MyAspect.OnSuccess(pipeline);
} 
catch(Exception e) {
  MyAspect.OnException(pipeline);
} 
finally {
  MyAspect.OnExit(pipeline);
}
```
В случае, когда на метод применено несколько аспектов, в указанных точках срабатывает каждый из них. Сами аспекты упорядочиваются по свойству ```Order``` в порядке убывания. Если порядок не задан, то вызываются сначала аспекты верхнего уровня (примененные на интерфейс), а затем локальные аспекты на методах.

2. **MethodInterceptAspect**. Пока не поддерживается.

## Управление пайплайном вызова
Модель пайплайна представлена в виде интерфейса [```IInvocationPipeline```](https://github.com/rex-core/IvorySharp/blob/master/IvorySharp/Aspects/Pipeline/IInvocationPipeline.cs), экземпляр реализации которого передается на вход каждой точке аспекта ```MethodBoundaryAspect```. Пайплайн в рамках библиотеки - это конвейер вызовов, включающий себя вызовы всех установленных на метод аспектов и вызов исходного метода. Доступны следующие механизмы взаимодействия с пайплайном
1. Вызов ```pipeline.Return()```  прекратит выполнение всего пайплайна и вернет клиенту исходного метода результат из ```pipeline.Context.ReturnValue```, если оно было задано.
2. Вызов ```pipeline.ReturnDefault()``` прекратит выполнение всего пайплайна и вернет клиенту исходного метода результат по умолчанию.
3. Вызов ```pipeline.ReturnValue(object)``` прекратит выполнение пайплайна и вернет клиенту заданный результат. Ответственность за приведение типов лежит на стороне аспекта, в случае неприводимости типа объекта заданного результата к возвращаемому типу метода будет сгенерировано исключение.
4. Вызов ```pipeline.Throw(Exception)``` прекратит выполнение пайплайна с исключением.
5. Обновление результата выполнения метода без прекращения пайплайна доступно в точках ```OnExit```, ```OnException```, ```OnSuccess``` через изменение значения свойства ```pipeline.Context.ReturnValue```.

## Передача состояния между аспектами
Для передачи состояния между вызовами в рамках одного аспекта используйте свойство ```IInvocationPipeline.AspectExecutionState```. Механизма для передачи состояния через весь пайплайн (между всеми аспектами) нет. Описанное выше свойство работает только на один аспект и другие аспекты изменений не увидят. Пример использования представлен [тут](https://github.com/rex-core/IvorySharp/blob/master/IvorySharp.Examples/Aspects/CacheAspect.cs).

## Обработка исключений
1. Если в процессе вызова исходного метода произошло исключение, то оно передается на вход аспекта в метод ```OnException```. Если ни один аспект не установлен на метод - исключение с сохранением стека будет проброшено в вызывающий код.
2. Аспект может *скрыть* исключение вызвав любую из версий ```pipeline.Return()```. В этом случае исключение будет поглощено и клиентский код продолжит свою работу как ни в чем не бывало. Однако, если ожидается какой-то результат выполнения метода, то возврат таким образом может привести к возникновению ```NullReferenceException```.
3. Аспект может подменить исключение вызвав ```pipeline.Throw(Exception)```, в этом случае старое исключение будет затерто, а клиентский код получит новое исключение, установленное обработчиком.
4. Если аспект не выполнит никаких действий по обработке исключений, то исключение будет передано на вход следующему аспекту. Если ни один из аспектов не остановил прокидывание исключения, то оно будет выброшено в клиентский код без потери стек-трейса.
5. Если в процессе выполнения методов аспектов произойдет исключение, то оно будет выброшено во вне. Поэтому обработка исключений на уровне точек выполнения аспекта лежит на пользователях библиотеки.


## Связывание объектов с аспектами
Наиболее прозрачным образом библиотека работает с паре с фреймворками по внедрению зависимостей, так как они берут работу по инъекции аспектов в исходные классы на себя. Впрочем, библиотеку можно использовать и без контейнеров (с некоторыми органичениями).

По умолчанию выполняется связывание, если на интерфейсе или на каком-либо его методе установлен атрибут, который наследуется от ```MethodAspect```. Отключить применение аспектов на интерфейсе целиком или на отдельных методах можно, установив атрибут ```[SuppressWeaving]```. Так же можно установить отдельный атрибут-маркер, по которому будет определяться необходимость применения аспектов на тип (см. секцию Настройки). 

## Интеграция со сторонними фрейворками
На данный момент реализована интеграция с библиотеками [SimpleInjector](https://simpleinjector.org/index.html) и [Castle.Windsor](http://www.castleproject.org/projects/windsor/). При этом в реализации для Castle использовано проксирование через родной для фреймворка компонент [Castle.DynamicProxy](http://www.castleproject.org/projects/dynamicproxy/).

Инициализация в общем случае имеет следующий вид. 
``` C#
public static void Main() { 

  var container = new Container();

  AspectsConfigurator
    .UseContainer(new SimpleInjectorAspectContainer(container))
    .Initialize();
}
``` 
При этом **инициализацию контейнера аспектов нужно проводить до регистации зависимостей в исходный контейнер.**

### Работа с зависимостями
По причинам того, что базовый класс аспекта является атрибутом внедрением зависимостей через конструктор не представляется возможным. Для получения завиcимостей доступен провайдер сервисов (сервис локатор), который можно получить в любом из методов аспекта, обратившись к свойству ```pipeline.ServiceProvider```. 

**Предупреждение**

Использование сервис провайдера может привести к дедлоку, когда аспект будет пытаться обратиться к методу сервиса, на котором этот аспект и установлен.

## Настройки
Конфигурация аспектов доступна во время инициализации контейнера через вызов перегруженного метода ```Inialize(Action<AspectConfiguration> configuration)```. Доступные настройки
* ```configuration.UseExplicitWeavingAttribute<T>``` - Устанавливает явное использование аспектов. При установке этой настройки связывание аспектов будет происходить только на тех сервисах, где установлен соответствующий атрибут. 
