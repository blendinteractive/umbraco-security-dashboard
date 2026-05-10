export interface AdvisoryPackageDto {
  packageName: string;
  affectedVersionRange: string;
  installedVersion: string | null;
  affectedStatus: 'Vulnerable' | 'Mitigated' | 'NotAffected' | 'Unknown';
}

export interface AdvisoryDto {
  ghsaId: string;
  title: string;
  severity: 'Critical' | 'High' | 'Moderate' | 'Low';
  advisoryUrl: string;
  publishedAt: string;
  affectedStatus: 'Vulnerable' | 'Mitigated' | 'NotAffected' | 'Unknown';
  packages: AdvisoryPackageDto[];
}

export interface DashboardStatusResponse {
  overallStatus: 'Safe' | 'Mitigated' | 'Vulnerable' | 'NeverChecked';
  isStale: boolean;
  lastSuccessfulCheckAt: string | null;
  lastCheckAttemptAt: string | null;
  lastCheckSucceeded: boolean | null;
  lastCheckError: string | null;
  nextScheduledCheckAt: string;
  affectedAdvisoryCount: number;
  mitigatedAdvisoryCount: number;
  advisories: AdvisoryDto[];
}
