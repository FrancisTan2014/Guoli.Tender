using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Guoli.Tender.Web.Models
{
    public sealed class Reply
    {
        public int Code { get; private set; }
        public string Msg { get; private set; }
        public object Data { get; private set; }

        private Reply() { }

        public static Reply Get(bool success)
        {
            return success ? OfSuccess() : OfFailed();
        }

        public static Reply OfParamsError()
        {
            return new Reply
            {
                Code = CodeConstrants.PARAMETERS_ERROR,
                Msg = "parameters illeagal",
            };
        }

        public static Reply OfSuccess(object data = null)
        {
            return new Reply
            {
                Code = CodeConstrants.SUCCESS,
                Msg = "processing success",
                Data = data
            };
        }

        public static Reply OfFailed()
        {
            return new Reply
            {
                Code = CodeConstrants.FAILED,
                Msg = "processing failed"
            };
        }

        public static Reply OfServerError()
        {
            return new Reply
            {
                Code = CodeConstrants.SERVER_ERROR,
                Msg = "server internal error"
            };
        }
    }

    /// <summary>
    /// 将消息码单独提出来，方便进行查阅
    /// </summary>
    public static class CodeConstrants
    {
        public const int PARAMETERS_ERROR = 100;
        public const int SUCCESS = 200;
        public const int FAILED = 201;
        public const int SERVER_ERROR = 500;
    }
}