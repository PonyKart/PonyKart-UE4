#ifndef KERNELOBJECT_H_INCLUDED
#define KERNELOBJECT_H_INCLUDED

namespace Ponykart
{
namespace LKernel
{
	class LKernelObject
	{
	protected:
		virtual ~LKernelObject () { }

		friend void shutdown ();
	};

} // LKernel
} // Ponykart
#endif // KERNELOBJECT_H_INCLUDED
