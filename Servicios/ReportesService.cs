using HoneyBack.Models;
using Microsoft.EntityFrameworkCore;

namespace HoneyBack.Servicios
{
    public class ReportesService : IReportesService
    {
        private readonly HoneyBalanceDbContext _context;

        public ReportesService(HoneyBalanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reporte>> ObtenerTodosAsync()
        {
            return await _context.Reportes
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.FechaReporte)
                .ToListAsync();
        }

        public async Task<Reporte?> ObtenerPorIdAsync(int id)
        {
            return await _context.Reportes
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.ReporteId == id);
        }

        public async Task<IEnumerable<Reporte>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Reportes
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.FechaReporte)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reporte>> ObtenerPorEstadoAsync(string estado)
        {
            return await _context.Reportes
                .Include(r => r.Usuario)
                .Where(r => r.Estado == estado)
                .OrderByDescending(r => r.FechaReporte)
                .ToListAsync();
        }

        public async Task<Reporte> CrearAsync(Reporte reporte)
        {
            reporte.FechaReporte = DateTime.Now;
            reporte.Estado = "Pendiente";
            _context.Reportes.Add(reporte);
            await _context.SaveChangesAsync();
            return reporte;
        }

        public async Task<Reporte?> ActualizarAsync(int id, Reporte reporte)
        {
            var reporteExistente = await _context.Reportes.FindAsync(id);
            if (reporteExistente == null)
                return null;

            reporteExistente.Nombre = reporte.Nombre;
            reporteExistente.TipoReporte = reporte.TipoReporte;
            reporteExistente.Descripcion = reporte.Descripcion;
            reporteExistente.Estado = reporte.Estado;

            await _context.SaveChangesAsync();
            return reporteExistente;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var reporte = await _context.Reportes.FindAsync(id);
            if (reporte == null)
                return false;

            _context.Reportes.Remove(reporte);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
