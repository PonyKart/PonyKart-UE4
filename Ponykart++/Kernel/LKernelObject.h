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

		friend void shutdownOgre ();
	};

} // LKernel
} // Ponykart
#endif // KERNELOBJECT_H_INCLUDED
