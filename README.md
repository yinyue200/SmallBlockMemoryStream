SmallBlockMemoryStream
======================

This assembly exposes a single class, `SmallBlockMemoryStream`, that is intended to be a drop-in replacement for the BCL `MemoryStream` class. The need for this class came to light at [Dow Jones](http://dowjones.com) while performance tuning high-capacity, high-availablity market data services for [MarketWatch](http://marketwatch.com), [The Wall Street Journal](http://online.wsj.com) and [Barron's](http://online.barrons.com). These services often return very large response messages, and preparing those messages was producing memory allocations on the [Large Object Heap](http://msdn.microsoft.com/en-us/magazine/cc534993.aspx) (LOH). When the LOH was eventually compacted, our services would pause for several seconds, and that would lead to mayhem in the data center (several seconds is an eternity to a high-capacity system).

We built the first version of this class to allow us to return large messages from WebApi actions without invoking the LOH. Along with configuration settings for the legacy WCF services, we were able eliminate the LOH from the picture entirely.

Usage
---

```cs
using Aethon.IO;

public class MyService()
{
  public Stream GetData()
  {
    var result = new SmallBlockMemoryStream();
    ...use just like a MemoryStream (see caveats below)...
    return result;
  }
}
```

The `SmallBlockMemoryStream` does not inherit from `MemoryStream` because:
  - there was little functionality that could be shared,
  - it would have increased the size of the implementation to carry it along as a base class, and
  - there are a few methods on `MemoryStream` that do not make sense in this context (`GetBuffer` and `ToArray`)
  
And that brings up a good point about these two classes: `SmallBlockMemoryStream` is not intended as a general replacement for `MemoryStream`. It is intended as a replacement when certain performce features are desired. In many cases, `MemoryStream` will be more performant than `SmallBlockMemoryStream`:

If|Then
-------------------|-------------------------
You know the final length of the stream and it will be <85K| `new MemoryStream(knownCapacity)`
You will need the contents of the stream as an array | `new MemoryStream()`
You know the final length of the stream and it will be >85K| `new SmallBlockMemoryStream(knownCapacity)`
The stream might end up >85K and you want to avoid the LOH | `new SmallBlockMemoryStream()`
The stream sizes will vary widely | Profile each class in your actual code

