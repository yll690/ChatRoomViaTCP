﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{

    //public enum CommandType
    //{
    //    //用户：登录
    //    Login = 'l',

    //    //服务器：登录结果
    //    LoginResult = 'L',

    //    //用户：注销
    //    Logout = 'O',

    //    //用户：注册
    //    SignUp = 's',

    //    //服务器：注册结果
    //    SignUpResult = 'S',

    //    //服务器：用户加入
    //    UserJoin = 'J',

    //    //服务器：用户退出
    //    UserQuit = 'Q',

    //    //用户、服务器：群聊消息
    //    GroupMessage = 'G',

    //    //用户、服务器：私聊消息
    //    PrivateMessage = 'P',

    //    //服务器：关闭
    //    ServerDisconnect = 'D',

    //    //服务器：移除用户
    //    Remove = 'R',
    //}
    public enum CommandType
    {
        //用户：登录
        Login,

        //服务器：登录结果
        LoginResult,

        //用户：注销
        Logout,

        //用户：注册
        SignUp,

        //服务器：注册结果
        SignUpResult,

        //服务器：用户加入
        UserJoin,

        //服务器：用户退出
        UserQuit,

        //用户、服务器：群聊消息
        GroupMessage,

        //用户、服务器：私聊消息
        PrivateMessage,

        //服务器：关闭
        ServerDisconnect,

        //服务器：移除用户
        Remove,
    }

}