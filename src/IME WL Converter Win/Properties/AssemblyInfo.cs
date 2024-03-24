/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过下列属性集
// 控制。更改这些属性值可修改
// 与程序集关联的信息。

[assembly: AssemblyTitle("深蓝词库转换")]
[assembly: AssemblyDescription(
    "随手写的一个词库转换小工具，支持主流的电脑和手机输入法，希望大家喜欢。\r\n有问题请联系我：studyzy@163.com\r\nGitHub: https://github.com/studyzy/imewlconverter"
)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("深蓝 http://studyzy.cnblogs.com")]
[assembly: AssemblyProduct("深蓝词库转换")]
[assembly: AssemblyCopyright("Copyright ©  2010-2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: InternalsVisibleTo("IME WL Converter Test")] //单元测试的时候可以访问内部对象
// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 属性设置为 true。

[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID

[assembly: Guid("93a0be45-6a70-41d4-85af-e3761b974664")]

// 程序集的版本信息由下面四个值组成:
//
//      主版本
//      次版本
//      内部版本号
//      修订号
//
// 可以指定所有这些值，也可以使用“内部版本号”和“修订号”的默认值，
// 方法是按如下所示使用“*”:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion(Studyzy.IMEWLConverter.ConstantString.VERSION)]
[assembly: AssemblyFileVersion(Studyzy.IMEWLConverter.ConstantString.VERSION)]
