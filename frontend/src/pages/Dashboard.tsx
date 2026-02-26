import { useEffect, useState } from 'react';
import { Package, AlertTriangle, DollarSign, TrendingUp } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { useGetDashboardQuery } from '../store/api';

interface LowStockItem { id: number; name: string; sku: string; stock: number; minStock: number; }
interface CategoryData { category: string | null; count: number; value: number; }

export default function InventoryDashboard() {
  const { data, isLoading } = useGetDashboardQuery();

  if (isLoading) return (
    <div className="flex justify-center items-center min-h-screen bg-gray-50">
      <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-indigo-600" />
    </div>
  );

  const kpis = [
    { label: 'Total Products', value: data?.totalProducts ?? 0, icon: Package, color: 'bg-blue-50 text-blue-600' },
    { label: 'Low Stock Alerts', value: data?.lowStockCount ?? 0, icon: AlertTriangle, color: 'bg-red-50 text-red-600' },
    { label: 'Inventory Value', value: `$${((data?.totalValue ?? 0) / 1000).toFixed(1)}k`, icon: DollarSign, color: 'bg-green-50 text-green-600' },
    { label: 'Categories', value: data?.byCategory?.length ?? 0, icon: TrendingUp, color: 'bg-purple-50 text-purple-600' },
  ];

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Inventory Dashboard</h1>
          <p className="text-gray-500 text-sm mt-1">Monitor stock levels and movements</p>
        </div>

        {/* KPIs */}
        <div className="grid grid-cols-4 gap-4 mb-8">
          {kpis.map(({ label, value, icon: Icon, color }) => (
            <div key={label} className="bg-white rounded-xl p-5 shadow-sm border border-gray-100">
              <div className={`w-10 h-10 rounded-lg flex items-center justify-center ${color} mb-3`}>
                <Icon size={20} />
              </div>
              <p className="text-2xl font-bold text-gray-900">{value}</p>
              <p className="text-sm text-gray-500">{label}</p>
            </div>
          ))}
        </div>

        <div className="grid grid-cols-3 gap-6">
          {/* Category Chart */}
          <div className="col-span-2 bg-white rounded-xl p-6 shadow-sm border border-gray-100">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Inventory by Category</h2>
            <ResponsiveContainer width="100%" height={220}>
              <BarChart data={(data?.byCategory ?? []).map((c: CategoryData) => ({ name: c.category || 'Uncategorized', count: c.count, value: +(c.value / 1000).toFixed(1) }))}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                <XAxis dataKey="name" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip />
                <Bar dataKey="count" fill="#4f46e5" radius={[4, 4, 0, 0]} name="Products" />
              </BarChart>
            </ResponsiveContainer>
          </div>

          {/* Low Stock */}
          <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Low Stock Alerts</h2>
            <div className="space-y-3">
              {(data?.lowStockItems ?? []).map((item: LowStockItem) => (
                <div key={item.id} className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-gray-800">{item.name}</p>
                    <p className="text-xs text-gray-400">{item.sku}</p>
                  </div>
                  <div className="text-right">
                    <span className="text-sm font-bold text-red-500">{item.stock}</span>
                    <p className="text-xs text-gray-400">min: {item.minStock}</p>
                  </div>
                </div>
              ))}
              {(data?.lowStockItems ?? []).length === 0 && (
                <p className="text-sm text-gray-400 text-center py-4">All stock levels healthy ✓</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
