//-----------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
//
//   For more information, see: http://go.microsoft.com/fwlink/?LinkID=623246
// </auto-generated>
//-----------------------------------------------------------------------------
#pragma once

namespace org { namespace OpenT2T { namespace Sample { namespace SuperPopular { namespace Shades {

public interface class IShadesService
{
public:
    // Implement this function to handle calls to the open method.
    Windows::Foundation::IAsyncOperation<ShadesOpenResult^>^ OpenAsync(_In_ Windows::Devices::AllJoyn::AllJoynMessageInfo^ info );

    // Implement this function to handle calls to the close method.
    Windows::Foundation::IAsyncOperation<ShadesCloseResult^>^ CloseAsync(_In_ Windows::Devices::AllJoyn::AllJoynMessageInfo^ info );

};

} } } } } 