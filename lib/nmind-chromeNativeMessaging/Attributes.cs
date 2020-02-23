//
// @author Nmind.io <osp@nmind.io>
// @licence MIT License
//
using System;

/// <summary>
/// 
/// </summary>
namespace Nmind.NativeMessaging {

    /// <summary>
    /// 
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class RequestMapping : Attribute {

        /// <summary>
        /// 
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        public RequestMapping(string route) {
            Route = route;
        }
    }
}
