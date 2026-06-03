export interface AdvisoryPackageDto {
  packageName: string;
  affectedVersionRange: string;
  installedVersion: string | null;
  affectedStatus: 'Vulnerable' | 'Mitigated' | 'NotAffected' | 'Unknown';
}

export interface ManualMitigationDto {
  description: string;
  mitigatedAt: string;
  mitigatedBy: string;
}

export interface AdvisoryDto {
  ghsaId: string;
  title: string;
  severity: 'Critical' | 'High' | 'Moderate' | 'Low';
  advisoryUrl: string;
  publishedAt: string;
  affectedStatus: 'Vulnerable' | 'Mitigated' | 'NotAffected' | 'Unknown';
  packages: AdvisoryPackageDto[];
  manualMitigation: ManualMitigationDto | null;
}

export interface AuditLogEntryDto {
  id: number;
  timestamp: string;
  overallStatus: 'Safe' | 'Mitigated' | 'Vulnerable' | 'NeverChecked';
  actionType: 'Manual' | 'Automatic';
  actorName: string | null;
  description: string;
}

export interface AuditLogPageResponse {
  entries: AuditLogEntryDto[];
  totalCount: number;
}

export interface DashboardStatusResponse {
  overallStatus: 'Safe' | 'Mitigated' | 'Vulnerable' | 'NeverChecked';
  isStale: boolean;
  scanningDisabled: boolean;
  lastSuccessfulCheckAt: string | null;
  lastCheckAttemptAt: string | null;
  lastCheckSucceeded: boolean | null;
  lastCheckError: string | null;
  nextScheduledCheckAt: string;
  affectedAdvisoryCount: number;
  mitigatedAdvisoryCount: number;
  advisories: AdvisoryDto[];
}
